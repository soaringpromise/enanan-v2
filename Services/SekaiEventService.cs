using System.Net;
using System.Net.Http.Json;
using EnananV2.Definitions.Models;
using Microsoft.Extensions.Logging;

namespace EnananV2.Services;

public sealed class SekaiEventService(HttpClient httpClient, ILogger<SekaiEventService> logger)
{
    private const string JpEventsUrl =
        "https://raw.githubusercontent.com/Sekai-World/sekai-master-db-diff/refs/heads/main/events.json";
    private const string EnEventsUrl =
        "https://raw.githubusercontent.com/Sekai-World/sekai-master-db-en-diff/refs/heads/main/events.json";

    private Dictionary<int, string>? _eventNames;

    private string? _jpEtag;
    private string? _enEtag;
    private List<EventAsset>? _lastJpEvents;
    private List<EventAsset>? _lastEnEvents;

    public async Task InitializeAsync() => await RefreshIfChangedAsync();

    public async Task RefreshIfChangedAsync(CancellationToken ct = default)
    {
        try
        {
            var (jpEvents, jpEtag, jpChanged) = await FetchEventsAsync(JpEventsUrl, _jpEtag, ct);

            List<EventAsset>? enEvents = null;
            var enEtag = _enEtag;
            var enChanged = false;

            try
            {
                (enEvents, enEtag, enChanged) = await FetchEventsAsync(EnEventsUrl, _enEtag, ct);
            }
            catch (Exception e)
            {
                logger.LogWarning(
                    e,
                    "Failed to refresh EN Sekai event data. Falling back to available JP data.");
            }

            _jpEtag = jpEtag;
            _enEtag = enEtag;

            if (!jpChanged && !enChanged && _eventNames is not null)
            {
                logger.LogDebug("Sekai event database unchanged.");
                return;
            }

            _lastJpEvents = jpEvents
                            ?? _lastJpEvents
                            ?? throw new InvalidOperationException("Failed to load JP event data.");

            _lastEnEvents = enEvents
                            ?? _lastEnEvents
                            ?? [];

            var enNames = _lastEnEvents
                .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                .ToDictionary(e => e.Id, e => e.Name!);

            _eventNames = _lastJpEvents.ToDictionary(
                jp => jp.Id,
                jp => enNames.TryGetValue(jp.Id, out var enName)
                    ? enName
                    : jp.Name ?? string.Empty);

            logger.LogInformation(
                "Sekai event database updated with {EventCount} events.",
                _eventNames.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to refresh Sekai event database.");
            if (_eventNames is null) throw;
        }
    }

    public bool EventExists(int eventId) => _eventNames?.ContainsKey(eventId) == true;

    public string GetEventName(int eventId)
    {
        if (_eventNames is null) throw new InvalidOperationException("SekaiEventService has not been initialized.");

        return !_eventNames.TryGetValue(eventId, out var name)
            ? throw new KeyNotFoundException($"Event ID {eventId} does not exist.")
            : name;
    }

    private async Task<(List<EventAsset>? Events, string? ETag, bool Changed)> FetchEventsAsync(
        string url, string? etag, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        if (etag is not null) request.Headers.TryAddWithoutValidation("If-None-Match", etag);

        using var response = await httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotModified) return (null, etag, false);

        response.EnsureSuccessStatusCode();
        var newEtag = response.Headers.ETag?.Tag;

        var events = await response.Content.ReadFromJsonAsync<List<EventAsset>>(cancellationToken: ct)
                     ?? throw new InvalidOperationException($"Failed to load event data from {url}.");

        return (events, newEtag, true);
    }
}