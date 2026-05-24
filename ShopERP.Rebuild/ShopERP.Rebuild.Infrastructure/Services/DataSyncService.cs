using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopERP.Rebuild.Core.Configuration;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Core.Domain.Enums;
using ShopERP.Rebuild.Infrastructure.Data;
using ShopERP.Rebuild.Infrastructure.Security;

namespace ShopERP.Rebuild.Infrastructure.Services;

public sealed class DataSyncService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IOptions<SyncOptions> options,
    ILogger<DataSyncService> logger) : ISyncService
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public SyncHealth CurrentHealth { get; private set; } = SyncHealth.Idle;
    public string LastMessage { get; private set; } = "Not synced yet.";
    public DateTime LastRunUtc { get; private set; } = DateTime.MinValue;

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            CurrentHealth = SyncHealth.Idle;
            LastMessage = "Sync disabled in settings.";
            return;
        }

        if (!await _gate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            CurrentHealth = SyncHealth.Running;
            using var scope = scopeFactory.CreateScope();
            var localDb = scope.ServiceProvider.GetRequiredService<ShopErpDbContext>();

            await localDb.Database.EnsureCreatedAsync(cancellationToken);

            var mysqlConnection = configuration.GetConnectionString("MySql")
                ?? throw new InvalidOperationException("MySql connection string is missing.");

            var remoteOptions = new DbContextOptionsBuilder<ShopErpDbContext>()
                .UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection))
                .Options;

            await using var remoteDb = new ShopErpDbContext(remoteOptions, Options.Create(options.Value));
            await remoteDb.Database.EnsureCreatedAsync(cancellationToken);

            var syncState = await localDb.SyncStates.SingleAsync(x => x.Id == 1, cancellationToken);
            var fromUtc = syncState.LastSyncedUtc;

            var products = await localDb.Products
                .Where(x => x.UpdatedUtc >= fromUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var customers = await localDb.Customers
                .Where(x => x.UpdatedUtc >= fromUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var sales = await localDb.Sales
                .Where(x => x.UpdatedUtc >= fromUtc)
                .Include(x => x.Lines)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var purchases = await localDb.Purchases
                .Where(x => x.UpdatedUtc >= fromUtc)
                .Include(x => x.Lines)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var protector = new AesProtector(options.Value.AesKey);
            _ = protector.Protect($"{products.Count}:{customers.Count}:{sales.Count}:{purchases.Count}");

            await UpsertRangeAsync(remoteDb.Products, products, cancellationToken);
            await UpsertRangeAsync(remoteDb.Customers, customers, cancellationToken);
            await UpsertSalesAsync(remoteDb, sales, cancellationToken);
            await UpsertPurchasesAsync(remoteDb, purchases, cancellationToken);

            await remoteDb.SaveChangesAsync(cancellationToken);

            syncState.LastSyncedUtc = DateTime.UtcNow;
            syncState.LastError = string.Empty;
            await localDb.SaveChangesAsync(cancellationToken);

            LastRunUtc = DateTime.UtcNow;
            CurrentHealth = SyncHealth.Success;
            LastMessage = $"Synced at {LastRunUtc:u}. Products={products.Count}, Customers={customers.Count}, Sales={sales.Count}, Purchases={purchases.Count}";

            var totalItems = products.Count + customers.Count + sales.Count + purchases.Count;
            if (totalItems > 0)
            {
                logger.LogInformation("{Message}", LastMessage);
            }
            else
            {
                logger.LogDebug("Sync completed with no data changes. fromUtc={FromUtc:u}", fromUtc);
            }
        }
        catch (Exception ex)
        {
            CurrentHealth = SyncHealth.Failed;
            LastMessage = ex.Message;
            logger.LogError(ex, "Sync failed");

            using var scope = scopeFactory.CreateScope();
            var localDb = scope.ServiceProvider.GetRequiredService<ShopErpDbContext>();
            var syncState = await localDb.SyncStates.SingleAsync(x => x.Id == 1, cancellationToken);
            syncState.LastError = ex.Message;
            await localDb.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task UpsertRangeAsync<T>(DbSet<T> dbSet, List<T> items, CancellationToken ct)
        where T : EntityBase
    {
        foreach (var item in items)
        {
            var existing = await dbSet.FindAsync([item.Id], ct);
            if (existing is null)
            {
                dbSet.Add(item);
                continue;
            }

            var values = dbSet.Entry(existing).CurrentValues;
            values.SetValues(item);
        }
    }

    private static async Task UpsertSalesAsync(ShopErpDbContext remoteDb, List<Sale> sales, CancellationToken ct)
    {
        foreach (var sale in sales)
        {
            var existing = await remoteDb.Sales.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == sale.Id, ct);
            if (existing is null)
            {
                remoteDb.Sales.Add(sale);
                continue;
            }

            remoteDb.Entry(existing).CurrentValues.SetValues(sale);
            remoteDb.SaleLines.RemoveRange(existing.Lines);

            foreach (var line in sale.Lines)
            {
                remoteDb.SaleLines.Add(line);
            }
        }
    }

    private static async Task UpsertPurchasesAsync(ShopErpDbContext remoteDb, List<Purchase> purchases, CancellationToken ct)
    {
        foreach (var purchase in purchases)
        {
            var existing = await remoteDb.Purchases.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == purchase.Id, ct);
            if (existing is null)
            {
                remoteDb.Purchases.Add(purchase);
                continue;
            }

            remoteDb.Entry(existing).CurrentValues.SetValues(purchase);
            remoteDb.PurchaseLines.RemoveRange(existing.Lines);

            foreach (var line in purchase.Lines)
            {
                remoteDb.PurchaseLines.Add(line);
            }
        }
    }
}
