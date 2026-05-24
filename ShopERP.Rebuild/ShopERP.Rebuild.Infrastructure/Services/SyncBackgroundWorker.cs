using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopERP.Rebuild.Core.Configuration;
using ShopERP.Rebuild.Core.Contracts;

namespace ShopERP.Rebuild.Infrastructure.Services;

public sealed class SyncBackgroundWorker(
    ISyncService syncService,
    IOptions<SyncOptions> options,
    ILogger<SyncBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var seconds = Math.Max(options.Value.IntervalSeconds, 5);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await syncService.SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background sync execution failed");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }
}
