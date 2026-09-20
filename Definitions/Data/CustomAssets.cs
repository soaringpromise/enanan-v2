using EnananV2.Definitions.Models;

namespace EnananV2.Definitions.Data;

public static class CustomAssets
{
    private const int TotalWelcomeBanners = 8;

    private static readonly EmojiInfo[] EasterEggs =
    [
        new("nightcordat2500", 1462355324387463168),
        new("nooooat2500", 1462355326442405930)
    ];

    public static readonly EmojiInfo Twitter = new("twitter", 1460877444876861470);
    public static readonly EmojiInfo Discord = new("discord", 1460877434990891069);
    public static readonly EmojiInfo GitHub = new("github", 1460877442632646812);
    public static readonly EmojiInfo Bluesky = new("bluesky", 1460877433451581569);
    
    public static EmojiInfo? GetRandomEasterEgg()
    {
        if (Random.Shared.Next(2500) != 0) return null;
        const double primaryWeight = 0.75;

        return Random.Shared.NextDouble() < primaryWeight
            ? EasterEggs[0]
            : EasterEggs[1];
    }
    
    public static string GetRandomWelcomeBanner()
        => $"https://cdn.soaringpromise.moe/enanan/bot/banners/enawelcome{Random.Shared.Next(1, TotalWelcomeBanners + 1)}.webp";
    
    
}