using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using EnananV2.Tools;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using NodaTime;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace EnananV2.Discord.Interactions;

public sealed class TierProfileModalInteractions(
    ValidationService validator,
    DatabaseService database,
    TierProfileDraftService drafts,
    ILogger<TierProfileModalInteractions> logger)
    : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("profile-card-step-1")]
    public async Task Step1()
    {
        try
        {
            var userId = Context.User.Id;
            var draft = drafts.Get(userId, 1);
            var components = GetComponents();

            var server = EnumTools.Parse<GameServer>(GetSelectedValue(components, "main-server"));
            var usualTier = EnumTools.Parse<TierBracket>(GetSelectedValue(components, "usual-tier"));
            var playstyle = EnumTools.Parse<Playstyle>(GetSelectedValue(components, "playstyle"));

            var timezoneId = GetText(components, "timezone").Trim();

            var timezone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezoneId)
                           ?? throw new InvalidInputException(timezoneId, "Invalid timezone.");

            draft.Server = server;
            draft.TimeZone = timezone;
            draft.UsualTier = usualTier;
            draft.Playstyle = playstyle;

            drafts.Advance(draft, 1,2);
            await RespondWithStepButton("Step 1/4 completed.", 2);
        }
        catch (InvalidRequestException e)
        {
            await RespondWithStepButton(e.Message, 1, "Try Again");
        }
        catch (Exception e)
        {
            await HandleUnexpectedError(e);
        }
    }

    [ComponentInteraction("profile-card-step-2")]
    public async Task Step2()
    {
        try
        {
            var userId = Context.User.Id;
            var draft = drafts.Get(userId, 2);
            var components = GetComponents();

            var gameId = GetText(components, "game-id").Trim();

            var primaryTalent = ParsePositiveInt(
                GetText(components, "primary-team-talent"), "Primary team talent");

            var primaryIsv = GetText(components, "primary-team-isv").Trim();

            primaryIsv = validator.ValidateIsv(primaryIsv);

            draft.GameId = gameId;
            draft.Primary = new TeamInfo(primaryTalent, primaryIsv);

            drafts.Advance(draft,2, 3);
            await RespondWithStepButton("Step 2/4 completed.", 3);
        }
        catch (InvalidRequestException e)
        {
            await RespondWithStepButton(e.Message, 2, "Try Again");
        }
        catch (Exception e)
        {
            await HandleUnexpectedError(e);
        }
    }

    [ComponentInteraction("profile-card-step-3")]
    public async Task Step3()
    {
        try
        {
            var userId = Context.User.Id;
            var draft = drafts.Get(userId, 3);
            var components = GetComponents();

            draft.Heal = CreateOptionalTeam(
                GetText(components, "heal-team-talent"),
                GetText(components, "heal-team-isv"),
                "Heal");

            draft.Encore = CreateOptionalTeam(
                GetText(components, "encore-team-talent"),
                GetText(components, "encore-team-isv"),
                "Encore");

            drafts.Advance(draft, 3,4);
            await RespondWithStepButton("Step 3/4 completed.", 4);
        }
        catch (InvalidRequestException e)
        {
            await RespondWithStepButton(e.Message, 3, "Try Again");
        }
        catch (Exception e)
        {
            await HandleUnexpectedError(e);
        }
    }

    [ComponentInteraction("profile-card-step-4")]
    public async Task Step4()
    {
        var modifiesOriginalMessage = Context.Interaction.Message is not null;
        if (modifiesOriginalMessage) await RespondAsync(InteractionCallback.DeferredModifyMessage);
        else await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        try
        {
            var userId = Context.User.Id;
            var draft = drafts.Get(userId, 4);
            var components = GetComponents();

            var displayCardId = ParsePositiveInt(GetText(components, "display-card-id"), "Display card ID");

            var highestTierValue = GetOptionalSelectedValue(components, "highest-tier");

            TierBracket? highestTier = highestTierValue is null
                ? null
                : EnumTools.Parse<TierBracket>(highestTierValue);

            var eventId = ParseOptionalPositiveInt(
                GetText(components, "highest-event"), "Highest placement event ID");

            validator.ValidateCardId(displayCardId);
            validator.ValidateEventId(eventId);

            var profile = new TierProfile(
                userId,
                draft.UsualTier!.Value,
                draft.Server!.Value,
                draft.Playstyle!.Value,
                draft.Primary!,
                draft.Heal,
                draft.Encore,
                displayCardId,
                draft.TimeZone!,
                draft.GameId!,
                highestTier,
                eventId);

            if (!await database.TierProfile.Create(profile))
                throw new InvalidRequestException("There was an error creating your tier profile. Try again later.");

            drafts.Remove(userId);

            await ModifyFlowMessage("Your tier profile has been created successfully.", []);
        }
        catch (InvalidRequestException e)
        {
            await ModifyFlowMessage(e.Message, CreateStepButton(4, "Try Again"));
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Tier profile creation failed for user {UserId}.",
                Context.User.Id);

            drafts.Remove(Context.User.Id);

            await ModifyFlowMessage(
                "There was an unexpected error creating your tier profile. Run `/tier-profile create` to start again.",
                []);
        }
    }

    private TeamInfo? CreateOptionalTeam(string talentValue, string isvValue, string teamName)
    {
        var talentText = NullIfEmpty(talentValue);
        var isv = NullIfEmpty(isvValue);

        if (talentText is null && isv is null) return null;

        if (talentText is null || isv is null)
            throw new InvalidInputException(talentText ?? isv, $"{teamName} team requires both talent and ISV.");

        var talent = ParsePositiveInt(talentText, $"{teamName} team talent");
        isv = validator.ValidateIsv(isv);
        return new TeamInfo(talent, isv);
    }

    private ILabelComponent[] GetComponents()
    {
        return
        [
            .. Context.Components
                .OfType<Label>()
                .Select(label => label.Component)
        ];
    }

    private static string GetText(IEnumerable<ILabelComponent> components, string customId)
    {
        return components.OfType<TextInput>().FirstOrDefault(input => input.CustomId == customId)?.Value
               ?? throw new InvalidRequestException($"Missing modal field: {customId}.");
    }

    private static string GetSelectedValue(IEnumerable<ILabelComponent> components, string customId)
    {
        return GetOptionalSelectedValue(components, customId)
               ?? throw new InvalidRequestException($"No value was selected for {customId}.");
    }

    private static string? GetOptionalSelectedValue(IEnumerable<ILabelComponent> components, string customId)
    {
        var menu = components.OfType<StringMenu>().FirstOrDefault(menu => menu.CustomId == customId);

        return menu is null
            ? throw new InvalidRequestException($"Missing modal field: {customId}.")
            : menu.SelectedValues?.SingleOrDefault();
    }

    private static int ParsePositiveInt(string value, string fieldName)
    {
        if (!int.TryParse(value, out var result) || result <= 0)
            throw new InvalidInputException(value, $"{fieldName} must be a positive number.");
        return result;
    }

    private static int? ParseOptionalPositiveInt(string value, string fieldName)
    {
        var normalized = NullIfEmpty(value);
        return normalized is null ? null : ParsePositiveInt(normalized, fieldName);
    }

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task RespondWithStepButton(string message, int step, string label = "Continue")
    {
        IMessageComponentProperties[] components =
        [
            new ActionRowProperties().AddComponents(
                new ButtonProperties($"tier-profile-step-{step}", label, ButtonStyle.Primary))
        ];
        await RespondFlowMessage(message, components);
    }

    private async Task RespondWithoutButton(string message)
        => await RespondFlowMessage(message, []);

    private async Task RespondFlowMessage(string message, IMessageComponentProperties[] components)
    {
        if (Context.Interaction.Message is null)
        {
            var properties = new InteractionMessageProperties()
                .WithContent(message)
                .WithComponents(components)
                .WithFlags(MessageFlags.Ephemeral);

            await RespondAsync(InteractionCallback.Message(properties));
            return;
        }

        await RespondAsync(
            InteractionCallback.ModifyMessage(options =>
            {
                options.Content = message;
                options.Components = components;
                options.Embeds = [];
            }));
    }

    private static IMessageComponentProperties[] CreateStepButton(int step, string label = "Continue")
    {
        return
        [
            new ActionRowProperties().AddComponents(
                new ButtonProperties($"tier-profile-step-{step}", label, ButtonStyle.Primary))
        ];
    }

    private Task ModifyFlowMessage(string message, IMessageComponentProperties[] components)
    {
        return ModifyResponseAsync(options =>
        {
            options.Content = message;
            options.Components = components;
            options.Embeds = [];
        });
    }

    private async Task HandleUnexpectedError(Exception e)
    {
        logger.LogError(
            e,
            "Tier profile creation failed for user {UserId}.",
            Context.User.Id);

        drafts.Remove(Context.User.Id);

        await RespondWithoutButton(
            "There was an unexpected error creating your tier profile. Run `/tier-profile create` to start again.");
    }
}