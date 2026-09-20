using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using EnananV2.Tools;
using EnananV2.Tools.Factories;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EnananV2.Discord.Interactions;

public class GuildRegistrationModalInteraction(
    DatabaseService database,
    ValidationService validator,
    ResponseService responses,
    RoleSetupService roleSetup,
    ILogger<GuildRegistrationModalInteraction> logger)
    : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("admin-register-guild")]
    public async Task RegisterGuild()
    {
        var deferred = false;
        var registrationStarted = false;
        Guild? guild = null;
        
        try
        {
            guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
            deferred = true;

            await validator.ValidateGuildIsNotRegistered(guild.Id);

            var components = Context.Components
                .OfType<Label>()
                .Select(label => label.Component)
                .ToArray();

            var systemChannel =
                GetComponent<ChannelMenu>(components, "system-channel-select").SelectedValues?.SingleOrDefault() 
                ?? throw new InvalidRequestException("No system channel was selected.");

            var welcomeChannel =
                GetComponent<ChannelMenu>(components, "welcome-channel-select").SelectedValues?.SingleOrDefault();

            var roleChannel =
                GetComponent<ChannelMenu>(components, "role-channel-select").SelectedValues?.SingleOrDefault();

            var roleModeValue =
                GetComponent<StringMenu>(components, "role-mode-select").SelectedValues?.SingleOrDefault()
                ?? throw new InvalidRequestException("No role mode was selected.");

            var roleMode = EnumTools.Parse<RoleMode>(roleModeValue);

            var roleTypes =
                GetComponent<CheckboxGroup>(components, "role-type-select").CheckedValues;

            var requiresRoleChannel = roleMode != RoleMode.None || roleTypes.Count > 0;
            if (requiresRoleChannel && roleChannel is null)
                throw new InvalidRequestException("A role channel must be selected when any roles are enabled.");

            TextGuildChannel? textRoleChannel = null;

            if (roleChannel is not null)
            {
                if (!guild.Channels.TryGetValue(roleChannel.Id, out var channel) || channel is not TextGuildChannel textChannel) 
                    throw new InvalidRequestException("The selected role channel is invalid.");

                textRoleChannel = textChannel;
            }

            var registered = await database.Guild.Register(guild.Id, systemChannel.Id, roleMode);
            if (!registered) throw new InvalidOperationException($"Failed to register guild {guild.Id}.");
            
            registrationStarted = true;

            var userIds = guild.Users.Values.Where(user => !user.IsBot).Select(user => user.Id);

            await database.GuildMember.RegisterAllUsers(guild.Id, userIds);

            if (welcomeChannel is not null)
            {
                var assigned = await database.Guild.AssignWelcomeChannel(guild.Id, welcomeChannel.Id);
                if (!assigned) throw new InvalidOperationException($"Failed to assign welcome channel for guild {guild.Id}.");
            }

            if (textRoleChannel is not null) await roleSetup.SetupRolesAsync(guild, textRoleChannel, roleMode, roleTypes);

            if (!await database.Guild.FinalizeSetup(guild.Id))
                throw new InvalidOperationException($"Failed to finalize setup for guild {guild.Id}.");

            await responses.ModifyStatusResponse(Context, "Guild successfully registered.", ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            if (registrationStarted && guild != null)
                await TryRollbackRegistrationAsync(guild);

            if (deferred)
                await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
            else
                await responses.SendStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to register guild.");

            if (registrationStarted && guild != null)
                await TryRollbackRegistrationAsync(guild);

            if (deferred)
            {
                await responses.ModifyStatusResponse(
                    Context,
                    "There was an error registering the guild. Try again later or contact the developer.",
                    ResponseType.Error);
            }
            else
            {
                await responses.SendStatusResponse(
                    Context,
                    "There was an error registering the guild. Try again later or contact the developer.",
                    ResponseType.Error);
            }
        }
    }
    
    private static T GetComponent<T>(IEnumerable<ILabelComponent> components, string customId)
        where T : class, ILabelComponent
    {
        return components.OfType<T>().FirstOrDefault(component => 
                   component switch
                   { 
                       ChannelMenu menu => menu.CustomId == customId, 
                       StringMenu menu => menu.CustomId == customId,
                       CheckboxGroup group => group.CustomId == customId,
                       TextInput input => input.CustomId == customId,
                       _ => false
                   }) 
               ?? throw new InvalidRequestException($"Missing modal field: {customId}.");
    }
    
    private async Task TryRollbackRegistrationAsync(Guild guild)
    {
        try
        {
            await roleSetup.RecoverInterruptedSetupAsync(guild);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to clean up incomplete registration for guild {GuildId}.",
                guild.Id);
        }
    }
}