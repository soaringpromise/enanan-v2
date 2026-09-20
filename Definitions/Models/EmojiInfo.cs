namespace EnananV2.Definitions.Models;

public sealed record EmojiInfo(string Name, ulong Id)
{
    public string Display => $"<:{Name}:{Id}>";
}