using System.Net;
using System.Net.Http.Json;
using System.Collections.Concurrent;
using EnananV2.Definitions.Data;
using EnananV2.Definitions.Models;
using Microsoft.Extensions.Logging;

namespace EnananV2.Services;

public sealed class SekaiCardService(HttpClient httpClient, ILogger<SekaiCardService> logger)
{
    private const string CardsUrl =
        "https://raw.githubusercontent.com/Sekai-World/sekai-master-db-diff/refs/heads/main/cards.json";

    private const string ThumbnailBaseUrl =
        "https://storage.sekai.best/sekai-jp-assets/thumbnail/chara";

    private Dictionary<int, CardAsset>? _cards;
    private string? _etag;

    private readonly ConcurrentDictionary<int, string> _thumbnailCache = new();

    private class CardData
    {
        public int Id { get; init; }
        public int CharacterId { get; init; }
        public string AssetBundleName { get; init; } = null!;
    }

    public async Task InitializeAsync() => await RefreshIfChangedAsync();

    public async Task RefreshIfChangedAsync(CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, CardsUrl);
            if (_etag is not null) request.Headers.TryAddWithoutValidation("If-None-Match", _etag);
            using var response = await httpClient.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                logger.LogDebug("Sekai card database unchanged.");
                return;
            }

            response.EnsureSuccessStatusCode();
            _etag = response.Headers.ETag?.Tag;

            var cards = await response.Content.ReadFromJsonAsync<List<CardData>>(cancellationToken: ct)
                        ?? throw new InvalidOperationException("Failed to load Sekai card data.");

            _cards = cards.ToDictionary(
                card => card.Id,
                card => new CardAsset
                {
                    Id = card.Id,
                    AssetBundleName = card.AssetBundleName,
                    CharacterColor = CharacterCatalog.Characters[card.CharacterId].Color
                });

            logger.LogInformation("Sekai card database updated with {CardCount} cards.", cards.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to refresh Sekai card database.");
            if (_cards is null) throw;
        }
    }

    public bool CardExists(int cardId) => _cards?.ContainsKey(cardId) == true;

    public CardAsset GetCard(int cardId)
    {
        if (_cards is null) throw new InvalidOperationException("SekaiCardService has not been initialized.");

        return !_cards.TryGetValue(cardId, out var card)
            ? throw new KeyNotFoundException($"Card ID {cardId} does not exist.")
            : card;
    }

    public async Task<string> GetThumbnailUrlAsync(int cardId)
    {
        if (_thumbnailCache.TryGetValue(cardId, out var cachedUrl)) return cachedUrl;
        var assetBundleName = GetCard(cardId).AssetBundleName;
        var afterTraining = $"{ThumbnailBaseUrl}/{assetBundleName}_after_training.webp";
        using var request = new HttpRequestMessage(HttpMethod.Head, afterTraining);
        using var response = await httpClient.SendAsync(request);

        var url = response.IsSuccessStatusCode
            ? afterTraining
            : $"{ThumbnailBaseUrl}/{assetBundleName}_normal.webp";

        _thumbnailCache.TryAdd(cardId, url);
        return url;
    }
    
    public async Task<string> GetThumbnailUrlAsync(CardAsset card)
    {
        if (_thumbnailCache.TryGetValue(card.Id, out var cachedUrl)) return cachedUrl;
        var afterTraining = $"{ThumbnailBaseUrl}/{card.AssetBundleName}_after_training.webp";
        using var request = new HttpRequestMessage(HttpMethod.Head, afterTraining);
        using var response = await httpClient.SendAsync(request);

        var url = response.IsSuccessStatusCode
            ? afterTraining
            : $"{ThumbnailBaseUrl}/{card.AssetBundleName}_normal.webp";

        _thumbnailCache.TryAdd(card.Id, url);
        return url;
    }
}