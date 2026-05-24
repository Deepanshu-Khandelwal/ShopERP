using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Common;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Infrastructure.Sync;

public sealed class MySqlSyncService(ShopErpDbContext sourceDb, IOptions<MySqlSyncOptions> options) : IMySqlSyncService
{
    private readonly MySqlSyncOptions _options = options.Value;
    private const string SyncName = "ShopERP.SqliteToMySql";

    public async Task<MySqlSyncResult> SyncToMySqlAsync(bool fullSync, CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            return new MySqlSyncResult(false, "MySQL sync is disabled in configuration.", "disabled", string.Empty, 0, 0, null, DateTime.UtcNow);
        }

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            return new MySqlSyncResult(false, "MySQL connection string is missing.", "invalid", string.Empty, 0, 0, null, DateTime.UtcNow);
        }

        var serverVersion = ServerVersion.AutoDetect(_options.ConnectionString);
        var targetOptions = new DbContextOptionsBuilder<ShopErpDbContext>()
            .UseMySql(_options.ConnectionString, serverVersion)
            .Options;

        await using var targetDb = new ShopErpDbContext(targetOptions);

        if (fullSync)
        {
            await targetDb.Database.EnsureDeletedAsync(ct);
            await targetDb.Database.EnsureCreatedAsync(ct);
        }
        else
        {
            await targetDb.Database.EnsureCreatedAsync(ct);
        }

        await EnsureCheckpointTableAsync(targetDb, ct);
        var checkpointName = string.IsNullOrWhiteSpace(_options.CheckpointName) ? SyncName : _options.CheckpointName.Trim();
        var previousCheckpoint = fullSync ? null : await GetCheckpointAsync(targetDb, checkpointName, ct);

        var tablesSynced = 0;
        var rowsSynced = 0;

        rowsSynced += await SyncSetAsync<Supplier>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<Customer>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<Doctor>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<Product>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<StockBatch>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<StockMovement>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<PurchaseBill>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<PurchaseItem>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<SalesBill>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<SalesItem>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<PurchaseReturn>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<PurchaseReturnItem>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<SalesReturn>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<SalesReturnItem>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<PaymentEntry>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<CustomerLedgerEntry>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<SupplierLedgerEntry>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<PurchaseOrder>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<PurchaseOrderItem>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        rowsSynced += await SyncSetAsync<NotificationEntry>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;
        rowsSynced += await SyncSetAsync<BackupLog>(sourceDb, targetDb, fullSync, previousCheckpoint, ct); tablesSynced++;

        await SetCheckpointAsync(targetDb, checkpointName, DateTime.UtcNow, ct);

        var databaseName = targetDb.Database.GetDbConnection().Database;
        var mode = fullSync ? "full" : "incremental";

        return new MySqlSyncResult(
            true,
            "Sync completed successfully.",
            mode,
            databaseName,
            tablesSynced,
            rowsSynced,
            previousCheckpoint,
            DateTime.UtcNow);
    }

    private static async Task<int> SyncSetAsync<TEntity>(
        ShopErpDbContext source,
        ShopErpDbContext target,
        bool fullSync,
        DateTime? checkpointUtc,
        CancellationToken ct)
        where TEntity : EntityBase
    {
        if (fullSync)
        {
            var rows = await source.Set<TEntity>().AsNoTracking().ToListAsync(ct);
            if (rows.Count == 0)
            {
                return 0;
            }

            await target.Set<TEntity>().AddRangeAsync(rows, ct);
            await target.SaveChangesAsync(ct);
            target.ChangeTracker.Clear();
            return rows.Count;
        }

        var query = source.Set<TEntity>().AsNoTracking().AsQueryable();
        if (checkpointUtc.HasValue)
        {
            var checkpoint = checkpointUtc.Value;
            query = query.Where(x => x.CreatedAtUtc > checkpoint || x.UpdatedAtUtc > checkpoint);
        }

        var changes = await query.ToListAsync(ct);
        if (changes.Count == 0)
        {
            return 0;
        }

        foreach (var row in changes)
        {
            var existing = await target.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == row.Id, ct);
            if (existing is null)
            {
                await target.Set<TEntity>().AddAsync(row, ct);
            }
            else
            {
                target.Entry(existing).CurrentValues.SetValues(row);
            }
        }

        await target.SaveChangesAsync(ct);
        target.ChangeTracker.Clear();
        return changes.Count;
    }

    private static async Task EnsureCheckpointTableAsync(ShopErpDbContext targetDb, CancellationToken ct)
    {
        await targetDb.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS SyncCheckpoints (
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                SyncName VARCHAR(100) NOT NULL UNIQUE,
                LastSyncedAtUtc DATETIME(6) NOT NULL
            )
            """,
            ct);
    }

    private static async Task<DateTime?> GetCheckpointAsync(ShopErpDbContext targetDb, string syncName, CancellationToken ct)
    {
        await using var connection = targetDb.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT LastSyncedAtUtc FROM SyncCheckpoints WHERE SyncName = @syncName LIMIT 1";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@syncName";
        parameter.Value = syncName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(ct);
        if (result is null || result is DBNull)
        {
            return null;
        }

        return Convert.ToDateTime(result).ToUniversalTime();
    }

    private static async Task SetCheckpointAsync(ShopErpDbContext targetDb, string syncName, DateTime syncedAtUtc, CancellationToken ct)
    {
        await using var connection = targetDb.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO SyncCheckpoints (SyncName, LastSyncedAtUtc) VALUES (@syncName, @lastSyncedAtUtc) " +
            "ON DUPLICATE KEY UPDATE LastSyncedAtUtc = VALUES(LastSyncedAtUtc)";

        var syncNameParameter = command.CreateParameter();
        syncNameParameter.ParameterName = "@syncName";
        syncNameParameter.Value = syncName;
        command.Parameters.Add(syncNameParameter);

        var syncedAtParameter = command.CreateParameter();
        syncedAtParameter.ParameterName = "@lastSyncedAtUtc";
        syncedAtParameter.Value = syncedAtUtc;
        command.Parameters.Add(syncedAtParameter);

        await command.ExecuteNonQueryAsync(ct);
    }
}


