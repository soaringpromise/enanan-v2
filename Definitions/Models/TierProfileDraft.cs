using NodaTime;

namespace EnananV2.Definitions.Models;

public sealed class TierProfileDraft
{
    public int Step { get; set; } = 1;
    public DateTimeOffset ExpiresAt { get; set; }

    public GameServer? Server { get; set; }
    public DateTimeZone? TimeZone { get; set; }
    public TierBracket? UsualTier { get; set; }
    public Playstyle? Playstyle { get; set; }

    public string? GameId { get; set; }
    public TeamInfo? Primary { get; set; }

    public TeamInfo? Heal { get; set; }
    public TeamInfo? Encore { get; set; }

    public int? FavoriteCardId { get; set; }
    public TierBracket? HighestTier { get; set; }
    public int? EventId { get; set; }
}