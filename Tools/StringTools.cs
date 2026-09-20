using System.Text;
using System.Text.RegularExpressions;
using EnananV2.Definitions.Models;
using NodaTime;

namespace EnananV2.Tools;

public static partial class StringTools
{
    public static string FormatIsv(string? isv)
    {
        return isv is not null
            ? $"{isv} ({IsvTools.CalculateBoost(isv)}%)"
            : "—";
    }

    public static string GetUtcOffset(DateTimeZone timeZone, Instant? instant = null)
    {
        var offset = timeZone.GetUtcOffset(instant ?? SystemClock.Instance.GetCurrentInstant());

        var totalMinutes = offset.Seconds / 60;
        var sign = totalMinutes < 0 ? "-" : "+";
        var absoluteMinutes = Math.Abs(totalMinutes);

        return $"UTC{sign}{absoluteMinutes / 60:D2}:{absoluteMinutes % 60:D2}";
    }

    public static string NormalizeWhitespace(string input)
    {
        return string.IsNullOrEmpty(input)
            ? string.Empty
            : WhitespaceFixer().Replace(input.Trim(), " ");
    }

    public static string StringToIdentifier(string input)
    {
        input = NormalizeWhitespace(input);
        return IdentifierCleaner().Replace(input, "").Replace(" ", "-").ToLowerInvariant();
    }

    public static string AddFakeStats(string input)
    {
        var comments = Random.Shared.Next(10, 101);
        var likes = Random.Shared.Next(comments, 10001);
        var shares = Random.Shared.Next(10, likes + 1);
        var views = Random.Shared.Next(likes, 1000001);

        return
            $"{input}\n\n" +
            $"💬 **{comments}** ・ " +
            $"🔁 **{FormatNumber(shares)}** ・ " +
            $"❤️ **{FormatNumber(likes)}** ・ " +
            $"👁️ **{FormatNumber(views)}**";
    }

    private static string FormatNumber(int number)
    {
        return number switch
        {
            >= 1000000 => $"{number / 1000000d:0.#}M",
            >= 1000 => $"{number / 1000d:0.#}K",
            _ => number.ToString()
        };
    }

    public static string SafeFormat(string message, params object[] args)
    {
        if (args.Length == 0) return message;
        try { return string.Format(message, args); }
        catch (FormatException) { return message; }
    }
    
    public static string FormatTier(TierBracket tier) => tier == TierBracket.Other ? "Other" : $"T{(int)tier}";
    
    public static void AppendTeam(StringBuilder builder, string name, TeamInfo? team)
    {
        if (team is null)
        {
            builder.AppendLine($"**{name} Team:** —");
            return;
        }

        builder.AppendLine($"**{name} Team:** {team.Talent:N0} ・ {FormatIsv(team.Isv)}");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceFixer();
    
    [GeneratedRegex("[^a-zA-Z0-9 ]")]
    private static partial Regex IdentifierCleaner();
}