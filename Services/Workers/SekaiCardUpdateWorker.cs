using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnananV2.Services.Workers;

public sealed class SekaiCardUpdateWorker(
    SekaiCardService cardService,
    ILogger<SekaiCardUpdateWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try { await cardService.RefreshIfChangedAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Failed to poll Sekai card DB."); }
        }
    }
}