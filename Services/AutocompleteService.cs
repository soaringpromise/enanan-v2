using EnananV2.Definitions.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NodaTime;

namespace EnananV2.Services;

public sealed class AutocompleteService
{
    public class TimezoneProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
            ApplicationCommandInteractionDataOption option,
            AutocompleteInteractionContext context)
        {
            var input = option.Value ?? string.Empty;

            var choices = DateTimeZoneProviders.Tzdb.Ids
                .Where(id => id.Contains(input, StringComparison.InvariantCultureIgnoreCase))
                .Select(id => new ApplicationCommandOptionChoiceProperties(id, id))
                .Take(25);

            return new ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
        }

        public ValueTask<IEnumerable<StringMenuSelectOptionProperties>?> GetModalChoicesAsync(
            StringMenuSelectOptionProperties option,
            AutocompleteInteractionContext context)
        {
            var input = option.Value;
            
            var choices = DateTimeZoneProviders.Tzdb.Ids
                .Where(id => id.Contains(input, StringComparison.InvariantCultureIgnoreCase))
                .Select(id => new StringMenuSelectOptionProperties(id, id))
                .Take(25);
            
            return new ValueTask<IEnumerable<StringMenuSelectOptionProperties>?>(choices);
        }
    }

    public class TierProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
            ApplicationCommandInteractionDataOption option,
            AutocompleteInteractionContext context)
        {
            var input = option.Value ?? string.Empty;

            var choices = Enum.GetValues<TierBracket>()
                .Where(tier => tier.ToString().StartsWith(input, StringComparison.InvariantCultureIgnoreCase))
                .Select(tier => new ApplicationCommandOptionChoiceProperties(tier.ToString(), (int)tier))
                .Take(25);

            return new ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
        }
    }
}