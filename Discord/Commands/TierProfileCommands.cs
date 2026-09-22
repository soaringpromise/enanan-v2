using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using EnananV2.Tools;
using EnananV2.Tools.Factories;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NodaTime;

namespace EnananV2.Discord.Commands;

[SlashCommand("tier-profile", "Create, edit, or query your tier profile.")]
public sealed class TierProfileCommands(
    ValidationService validator,
    ResponseService responses,
    TierProfileDraftService drafts,
    DatabaseService database,
    SekaiCardService sekaiCardService,
    SekaiEventService sekaiEventService,
    ILogger<TierProfileCommands> logger)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "Create your custom tier profile.")]
    public async Task CreateTierProfile()
    {
        var draftStarted = false;
        try
        {
            await validator.UserAlreadyHasTierProfile(Context.User.Id);

            drafts.Start(Context.User.Id);
            draftStarted = true;

            await RespondAsync(InteractionCallback.Modal(ModalFactory.CreateProfileCardStep1Modal()));
        }
        catch (InvalidRequestException e)
        {
            if (draftStarted) drafts.Remove(Context.User.Id);
            await responses.SendStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            if (draftStarted) drafts.Remove(Context.User.Id);

            logger.LogError(
                e,
                "Failed to start tier profile creation for user {UserId}.",
                Context.User.Id);

            await responses.SendStatusResponse(
                Context,
                "There was an error starting profile creation.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("view", "View a tier profile.")]
    public async Task ViewTierProfile(
        [SlashCommandParameter(
            Name = "user",
            Description = "(Optional) View another user's tier profile.")]
        GuildUser? user = null)
    {
        var targetUser = user ?? (GuildUser)Context.User;
        await responses.DeferAsync(Context);
        try
        {
            var profile = await validator.ValidateTierProfileExists(targetUser.Id);
            var username = targetUser.GlobalName ?? targetUser.Username;

            var avatarUrl =
                (targetUser.GetGuildAvatarUrl(ImageFormat.WebP)
                 ?? targetUser.GetAvatarUrl(ImageFormat.WebP)
                 ?? targetUser.DefaultAvatarUrl).ToString();

            var card = sekaiCardService.GetCard(profile.FavoriteCardId);
            var cardUrl = await sekaiCardService.GetThumbnailUrlAsync(card);

            var eventName = profile.EventId.HasValue
                ? sekaiEventService.GetEventName(profile.EventId.Value)
                : null;

            var embed = EmbedFactory.CreateTierProfileEmbed(
                profile,
                username,
                avatarUrl,
                cardUrl,
                new Color(card.CharacterColor),
                eventName);

            await responses.ModifyEmbedResponse(Context, embed);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to display tier profile for user {UserId}.",
                targetUser.Id);

            await responses.ModifyStatusResponse(
                Context,
                "There was a problem displaying the tier profile. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }


    [SubSlashCommand("delete", "Delete your tier profile.")]
    public async Task DeleteTierProfile()
    {
        await responses.DeferAsync(Context, true);
        try
        {
            var userId = Context.User.Id;
            await validator.ValidateTierProfileExists(userId);

            if (!await database.TierProfile.Delete(userId))
                throw new InvalidRequestException("There was an error deleting your tier profile. Try again later.");

            await responses.ModifyStatusResponse(
                Context, "Your tier profile has been deleted.", ResponseType.Success);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to delete tier profile for user {UserId}.",
                Context.User.Id);

            await responses.ModifyStatusResponse(
                Context,
                "An unexpected error occurred while deleting your tier profile.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("edit", "Edit your tier profile.")]
    public sealed class EditTierProfileCommands(
        DatabaseService database,
        ValidationService validator,
        ResponseService responses,
        ILogger<EditTierProfileCommands> logger)
        : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("target", "Change your usual tier target.")]
        public async Task EditTierTarget(
            [SlashCommandParameter(Name = "tier", Description = "Your new usual tier target.")]
            TierBracket tier)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var userId = Context.User.Id;

                await validator.ValidateTierProfileExists(userId);

                if (!await database.TierProfile.UpdateTierTarget(userId, tier))
                    throw new InvalidRequestException(
                        "There was a problem updating your tier target. Try again later.");

                await responses.ModifyStatusResponse(
                    Context,
                    $"Your tier target has been updated to {tier}.",
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
                    "Failed to update tier target for user {UserId}.",
                    Context.User.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an unexpected error updating your tier target.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("server", "Change your usual event server.")]
        public async Task EditServer(
            [SlashCommandParameter(Name = "server", Description = "Your new active server.")]
            GameServer server)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var userId = Context.User.Id;

                await validator.ValidateTierProfileExists(userId);

                if (!await database.TierProfile.UpdateServer(userId, server))
                    throw new InvalidRequestException("There was a problem updating your server. Try again later.");

                await responses.ModifyStatusResponse(
                    Context, $"Your server has been updated to {EnumTools.GetDisplayName(server)}.", ResponseType.Success);
            }
            catch (InvalidRequestException e)
            {
                await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Failed to update server for user {UserId}.",
                    Context.User.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an unexpected error updating your server.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("timezone", "Change your current timezone.")]
        public async Task EditTimezone(
            [SlashCommandParameter(Name = "timezone", Description = "Your new timezone.",
                AutocompleteProviderType = typeof(AutocompleteService.TimezoneProvider))]
            string tz)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var userId = Context.User.Id;
                var timezoneId = tz.Trim();

                await validator.ValidateTierProfileExists(userId);

                var timezone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezoneId)
                               ?? throw new InvalidInputException(timezoneId, "Invalid timezone.");

                if (!await database.TierProfile.UpdateTimezone(userId, timezone))
                    throw new InvalidRequestException("There was a problem updating your timezone. Try again later.");

                await responses.ModifyStatusResponse(
                    Context, $"Your timezone has been updated to {timezone.Id}.", ResponseType.Success);
            }
            catch (InvalidRequestException e)
            {
                await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Failed to update timezone for user {UserId}.",
                    Context.User.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an unexpected error updating your timezone.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("team", "Change one of your teams.")]
        public async Task EditTeam(
            [SlashCommandParameter(Name = "team", Description = "The team to update.")]
            TeamSlot team,
            [SlashCommandParameter(Name = "talent", Description = "The team's talent.", MinValue = 1)]
            int talent,
            [SlashCommandParameter(Name = "isv", Description = "The team's ISV (e.g. 150/700).")]
            string isv)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var userId = Context.User.Id;
                isv = isv.Trim();

                await validator.ValidateTierProfileExists(userId);
                isv = validator.ValidateIsv(isv);

                if (!await database.TierProfile.UpdateTeam(userId, team, new TeamInfo(talent, isv))) 
                    throw new InvalidRequestException(
                        $"There was a problem updating your {team.ToString().ToLowerInvariant()} team. Try again later.");

                await responses.ModifyStatusResponse(
                    Context,
                    $"Your {team.ToString().ToLowerInvariant()} team has been updated to {talent:N0} / {StringTools.FormatIsv(isv)}.",
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
                    "Failed to update {Team} team for user {UserId}.",
                    team,
                    Context.User.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an unexpected error updating your team.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("playstyle", "Change your usual playstyle.")]
        public async Task EditPlaystyle(
            [SlashCommandParameter(Name = "playstyle", Description = "Your new playstyle.")]
            Playstyle playstyle)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var userId = Context.User.Id;

                await validator.ValidateTierProfileExists(userId);

                if (!await database.TierProfile.UpdatePlaystyle(userId, playstyle))
                    throw new InvalidRequestException("There was a problem updating your playstyle. Try again later.");

                await responses.ModifyStatusResponse(
                    Context,
                    $"Your playstyle has been updated to {EnumTools.GetDisplayName(playstyle)}.",
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
                    "Failed to update playstyle for user {UserId}.",
                    Context.User.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an unexpected error updating your playstyle.",
                    ResponseType.Error);
            }
        }

        [SubSlashCommand("card", "Change your displayed card.")]
        public async Task EditDisplayCard(
            [SlashCommandParameter(Name = "card", Description = "The card ID to display on your profile.", MinValue = 1)]
            int cardId)
        {
            await responses.DeferAsync(Context, true);
            try
            {
                var userId = Context.User.Id;

                await validator.ValidateTierProfileExists(userId);
                validator.ValidateCardId(cardId);

                if (!await database.TierProfile.UpdateFavoriteCard(userId, cardId))
                    throw new InvalidRequestException("There was a problem updating your displayed card. Try again later.");

                await responses.ModifyStatusResponse(
                    Context,
                    $"Your displayed card has been updated to #{cardId}.",
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
                    "Failed to update display card for user {UserId}.",
                    Context.User.Id);

                await responses.ModifyStatusResponse(
                    Context,
                    "There was an unexpected error updating your displayed card.",
                    ResponseType.Error);
            }
        }
    }
}