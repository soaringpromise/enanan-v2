using System.Net;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EnananV2.Discord.Interactions;

public class StringMenuInteraction(
    ResponseService responses,
    ValidationService validator)
    : ComponentInteractionModule<StringMenuInteractionContext>
{
    [ComponentInteraction("characterRoleSelect")]
    public Task CharacterRoleSelect() => ToggleRoleAsync();

    [ComponentInteraction("unitRoleSelect")]
    public Task UnitRoleSelect() => ToggleRoleAsync();

    [ComponentInteraction("tieringRoleSelect")]
    public Task TieringRoleSelect() => ToggleRoleAsync();

    [ComponentInteraction("identityRoleSelect")]
    public Task IdentityRoleSelect() => ToggleRoleAsync();

    [ComponentInteraction("utilityRoleSelect")]
    public Task UtilityRoleSelect() => ToggleRoleAsync();

    private async Task ToggleRoleAsync()
    {
        if (Context.Guild is null)
        {
            await responses.SendStatusResponse(
                Context, "This menu can only be used inside a server.", ResponseType.Warning);

            return;
        }

        await validator.ValidateGuildAccess(Context.Guild.Id);

        var selectedValue = Context.SelectedValues.FirstOrDefault();

        if (selectedValue is null || !ulong.TryParse(selectedValue, out var roleId))
        {
            await responses.SendStatusResponse(Context, "The selected role is invalid.", ResponseType.Warning);
            return;
        }

        if (!Context.Guild.Roles.ContainsKey(roleId))
        {
            await responses.SendStatusResponse(Context, "That role no longer exists.", ResponseType.Warning);
            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage);

        string feedback;

        try
        {
            var member = await Context.Guild.GetUserAsync(Context.User.Id);

            if (member.RoleIds.Contains(roleId))
            {
                await member.RemoveRoleAsync(roleId);
                feedback = "Role removed.";
            }
            else
            {
                await member.AddRoleAsync(roleId);
                feedback = "Role added!";
            }
        }
        catch (RestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            feedback = "That role no longer exists.";
        }

        await Context.Interaction.SendFollowupMessageAsync(
            new InteractionMessageProperties().WithContent(feedback).WithFlags(MessageFlags.Ephemeral));
    }
}