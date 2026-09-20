using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using EnananV2.Tools.Factories;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EnananV2.Discord.Commands;

[SlashCommand("admin", "Admin commands.", DefaultGuildPermissions = Permissions.Administrator)]
public sealed class AdministratorCommands(
    ValidationService validator,
    ResponseService responses,
    DatabaseService database,
    RoleSetupService roleSetup,
    CustomRoleService customRoles,
    ILogger<AdministratorCommands> logger)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("register", "Register guild.")]
    public async Task RegisterGuild()
    {
        try
        {
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");
            
            if (await database.Guild.Exists(guild.Id) && !await database.Guild.IsSetupComplete(guild.Id)) 
                await roleSetup.RecoverInterruptedSetupAsync(guild);

            await validator.ValidateGuildIsNotRegistered(guild.Id);
            var registrationModal = ModalFactory.CreateRegistrationModal();
            await RespondAsync(InteractionCallback.Modal(registrationModal));
        }
        catch (InvalidRequestException e)
        {
            await responses.SendStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to open the guild registration modal.");

            await responses.SendStatusResponse(
                Context,
                "There was an error starting guild registration. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("system", "Sets or updates the system message channel.")]
    public async Task UpdateSystemChannel(
        [SlashCommandParameter(
            Name = "channel",
            Description = "The channel where system messages will be sent.")]
        TextGuildChannel channel)
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateGuildIsRegistered(guild.Id);

            if (!await database.Guild.UpdateSystemChannel(guild.Id, channel.Id))
                throw new InvalidRequestException("There was a problem updating the system channel. Try again later.");

            await responses.ModifyStatusResponse(
                Context,
                $"System messages will now be sent in <#{channel.Id}>.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to update system channel for guild {GuildId}.",
                Context.Interaction.Guild?.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was an error updating the system channel. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("welcome", "Sets or updates the welcome channel.")]
    public async Task UpdateWelcomeChannel(
        [SlashCommandParameter(
            Name = "channel", Description = "The channel where welcome messages will be sent.")]
        TextGuildChannel channel)
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateGuildIsRegistered(guild.Id);

            if (!await database.Guild.AssignWelcomeChannel(guild.Id, channel.Id))
                throw new InvalidRequestException("There was a problem updating the welcome channel. Try again later.");

            await responses.ModifyStatusResponse(
                Context,
                $"Welcome messages will now be sent in <#{channel.Id}>.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to update welcome channel for guild {GuildId}.",
                Context.Interaction.Guild?.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was an error updating the welcome channel. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("welcome-delete", "Disables welcome messages for this server.")]
    public async Task DeleteWelcomeChannel()
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateGuildIsRegistered(guild.Id);

            if (!await database.Guild.RemoveWelcomeChannel(guild.Id))
                throw new InvalidRequestException("There was a problem removing the welcome channel. Try again later.");

            await responses.ModifyStatusResponse(
                Context,
                "Welcome messages have been disabled.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to remove welcome channel for guild {GuildId}.",
                Context.Interaction.Guild?.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was an error removing the welcome channel. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }
    
    [SubSlashCommand("register-user", "Manually register a user in this server.")]
    public async Task RegisterUser(
        [SlashCommandParameter(Name = "user", Description = "The user to register.")]
        GuildUser user)
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateGuildIsRegistered(guild.Id);

            if (user.IsBot) throw new InvalidRequestException("Bots cannot be registered.");

            if (!await database.GuildMember.RegisterUser(guild.Id, user.Id))
                throw new InvalidRequestException("This user is already registered in this server.");

            var username = user.Nickname ?? user.GlobalName ?? user.Username;

            await responses.ModifyStatusResponse(
                Context, $"**{username}** has been registered successfully.", ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to manually register user {UserId} in guild {GuildId}.",
                user.Id,
                Context.Interaction.Guild?.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was an error registering the user. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }
    
    [SubSlashCommand("unregister-user", "Manually unregister a user from this server.")]
    public async Task UnregisterUser(
        [SlashCommandParameter(
            Name = "user-id", Description = "The Discord ID of the user to unregister.",
            MinLength = 17, MaxLength = 20)]
        string userIdString)
    {
        await responses.DeferAsync(Context, true);

        try
        {
            if (!ulong.TryParse(userIdString, out var userId))
                throw new InvalidInputException(userIdString, "Invalid Discord user ID.");

            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateGuildIsRegistered(guild.Id);

            var roleId = await database.GuildMember.GetCustomRoleId(guild.Id, userId);

            if (roleId is not null) await customRoles.DeleteCustomRoleAsync(guild, userId, roleId.Value);

            if (!await database.GuildMember.DeleteUser(guild.Id, userId))
                throw new InvalidRequestException("That user is not registered in this server.");

            await responses.ModifyStatusResponse(
                Context,
                $"User `{userId}` has been unregistered successfully.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to manually unregister user {UserId} from guild {GuildId}.",
                userIdString,
                Context.Interaction.Guild?.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was an error unregistering the user. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("role", "Manage user custom roles.")]
    public sealed class AdminRoleCommands(
        CustomRoleService customRoles,
        ValidationService validator,
        ResponseService responses,
        ILogger<AdminRoleCommands> logger)
        : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("create", "Create a custom role for a user.")]
        public async Task CreateRole(
            [SlashCommandParameter(Name = "user", Description = "The user to create the custom role for.")]
            GuildUser user,
            [SlashCommandParameter(Name = "title", MinLength = 2, MaxLength = 64, Description = "The title for the custom role.")]
            string title,
            [SlashCommandParameter(Name = "color", MinLength = 3, Description = "The color as a hex code or an existing color name.")]
            string colorString)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

                await validator.ValidateCustomRoleAccess(guild.Id);
                await validator.ValidateUserDoesNotOwnCustomRole(guild, user.Id);

                validator.ValidateGuildCanCreateRole(guild.Roles.Count);
                title = validator.ValidateRoleTitle(title);
                validator.ValidateColor(colorString, out var color);

                await customRoles.CreateCustomRoleAsync(guild, user, Context.Client.Id, title, color);

                var username = user.Nickname ?? user.GlobalName ?? user.Username;

                await responses.ModifyStatusResponse(
                    Context, $"Created a custom role for **{username}**.", ResponseType.Success);
            }
            catch (InvalidRequestException e)
            {
                await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Failed to create custom role for user {UserId}.",
                    user.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an error creating the custom role. Try again later or contact the developer.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("edit", "Update a user's custom role.")]
        public async Task UpdateRole(
            [SlashCommandParameter(Name = "user", Description = "The user whose custom role will be updated.")]
            GuildUser user,
            [SlashCommandParameter(Name = "new-title", MinLength = 2, MaxLength = 64, Description = "The new title for the custom role.")]
            string? newTitle = null,
            [SlashCommandParameter(Name = "new-color", MinLength = 3, Description = "The new color as a hex code or an existing color name.")]
            string? newColor = null)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

                await validator.ValidateCustomRoleAccess(guild.Id);

                var roleId = await validator.ValidateUserOwnsCustomRole(guild, user.Id);

                validator.ValidateEditRequest(newTitle, newColor);
                newTitle = validator.ValidateOptionalRoleTitle(newTitle);
                validator.ValidateOptionalColor(newColor, out var color);

                await customRoles.EditCustomRoleAsync(guild, roleId, newTitle, color);
                var username = user.Nickname ?? user.GlobalName ?? user.Username;

                await responses.ModifyStatusResponse(
                    Context, $"Updated **{username}**'s custom role.", ResponseType.Success);
            }
            catch (InvalidRequestException e)
            {
                await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Failed to update custom role for user {UserId}.",
                    user.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an error updating the custom role. Try again later or contact the developer.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("delete", "Delete a user's custom role.")]
        public async Task DeleteRole(
            [SlashCommandParameter(Name = "user", Description = "The user whose custom role will be deleted.")]
            GuildUser user)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

                await validator.ValidateCustomRoleAccess(guild.Id);

                var roleId = await validator.ValidateUserOwnsCustomRole(guild, user.Id);

                await customRoles.DeleteCustomRoleAsync(guild, user.Id, roleId);

                var username = user.Nickname ?? user.GlobalName ?? user.Username;

                await responses.ModifyStatusResponse(
                    Context, $"Deleted **{username}**'s custom role.", ResponseType.Success);
            }
            catch (InvalidRequestException e)
            {
                await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Failed to delete custom role for user {UserId}.",
                    user.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an error deleting the custom role. Try again later or contact the developer.",
                    ResponseType.Error);
            }
        }
    }
}