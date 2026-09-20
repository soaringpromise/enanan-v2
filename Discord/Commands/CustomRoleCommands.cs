using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EnananV2.Discord.Commands;

[SlashCommand(name: "role", description: "Manage your custom roles!")]
public sealed class CustomRoleCommands(
    CustomRoleService customRoles,
    ValidationService validator,
    ResponseService responses,
    ILogger<CustomRoleCommands> logger)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "Create a new custom role.")]
    public async Task CreateCustomRole(
        [SlashCommandParameter(Name = "title", MinLength = 2, MaxLength = 64, Description = "The title for your custom role.")]
        string title,
        [SlashCommandParameter(Name = "color", MinLength = 3, Description = "The color as a hex code or an existing color name.")]
        string colorString)
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var user = (GuildUser)Context.Interaction.User;
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateCustomRoleAccess(guild.Id);
            await validator.ValidateUserDoesNotOwnCustomRole(guild, user.Id);

            validator.ValidateGuildCanCreateRole(guild.Roles.Count);
            title = validator.ValidateRoleTitle(title);
            validator.ValidateColor(colorString, out var color);

            await customRoles.CreateCustomRoleAsync(guild, user, Context.Client.Id, title, color);
            var username = user.Nickname ?? user.GlobalName ?? user.Username;

            await responses.ModifyStatusResponse(
                Context,
                $"Congrats, **{username}**! Your custom role has been created.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create custom role.");

            await responses.ModifyStatusResponse(
                Context,
                "There was an error creating your custom role. Contact an administrator.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("edit", "Edit your custom role.")]
    public async Task EditCustomRole(
        [SlashCommandParameter(Name = "new-title", MinLength = 2, MaxLength = 64,
            Description = "The NEW title for your custom role.")]
        string? newTitle = null,
        [SlashCommandParameter(Name = "new-color", MinLength = 3,
            Description = "The NEW color as a hex code or an existing color name.")]
        string? newColor = null)
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var user = (GuildUser)Context.Interaction.User;
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateCustomRoleAccess(guild.Id);
            var roleId = await validator.ValidateUserOwnsCustomRole(guild, user.Id);

            validator.ValidateEditRequest(newTitle, newColor);
            newTitle = validator.ValidateOptionalRoleTitle(newTitle);
            validator.ValidateOptionalColor(newColor, out var color);

            await customRoles.EditCustomRoleAsync(guild, roleId, newTitle, color);

            var username = user.Nickname ?? user.GlobalName ?? user.Username;

            await responses.ModifyStatusResponse(
                Context,
                $"Nice, **{username}**! Your custom role has been updated.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to edit custom role.");

            await responses.ModifyStatusResponse(
                Context,
                "There was an error updating your custom role. Contact an administrator.",
                ResponseType.Error);
        }
    }


    [SubSlashCommand("delete", "Delete your custom role.")]
    public async Task DeleteCustomRole()
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var user = (GuildUser)Context.Interaction.User;
            var guild = Context.Interaction.Guild ?? throw new InvalidRequestException("No guild provided.");

            await validator.ValidateCustomRoleAccess(guild.Id);
            var roleId = await validator.ValidateUserOwnsCustomRole(guild, user.Id);

            await customRoles.DeleteCustomRoleAsync(guild, user.Id, roleId);

            var username = user.Nickname ?? user.GlobalName ?? user.Username;

            await responses.ModifyStatusResponse(
                Context,
                $"Your custom role has been deleted, **{username}**.",
                ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete custom role for user {UserId}.", Context.Interaction.User.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was an error deleting your custom role. Contact an administrator.",
                ResponseType.Error);
        }
    }
}