using NodaTime;

namespace EnananV2.Definitions.Models;

public record TierProfile(
    ulong UserId,

    TierBracket UsualTier,
    GameServer Server,
    Playstyle Playstyle,

    TeamInfo Primary,
    TeamInfo? Heal,
    TeamInfo? Encore,

    int FavoriteCardId,

    DateTimeZone TimeZone,
    string GameId,
    
    TierBracket? HighestTier,
    int? EventId);