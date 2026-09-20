using System.Text.Json.Serialization;

namespace EnananV2.Definitions.Models;

public sealed record EventAsset
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}