namespace EnananV2.Definitions.Models;

public sealed record CardAsset
{
    public int Id { get; init; }
    public string AssetBundleName { get; init; } = string.Empty;
    public int CharacterColor { get; init; }
}