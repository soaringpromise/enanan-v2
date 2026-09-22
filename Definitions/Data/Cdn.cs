namespace EnananV2.Definitions.Data;

public static class Cdn
{
    private const string BaseUrl = "https://cdn.soaringpromise.moe/enanan/bot";

    public static string Banner(string fileName)
        => $"{BaseUrl}/banners/{fileName}.webp";

    public static string Unit(string fileName)
        => $"{BaseUrl}/units/{fileName}.webp";

    public static string Ena(string fileName)
        => $"{BaseUrl}/ena/{fileName}.webp";

    public static string Status(string fileName)
        => $"{BaseUrl}/status/{fileName}.webp";
}