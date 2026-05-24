using System.Data;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Data;

public class ShopErpDbContext(DbContextOptions<ShopErpDbContext> options) : DbContext(options)
{
    private static readonly string[] SqlitePerformancePragmas =
    [
        "PRAGMA journal_mode = WAL",
        "PRAGMA synchronous = NORMAL",
        "PRAGMA temp_store = MEMORY",
        "PRAGMA cache_size = -20000"
    ];

    private static readonly (string TableName, string Sql)[] SqlitePerformanceIndexes =
    [
        ("StockBatches", "CREATE INDEX IF NOT EXISTS IX_StockBatches_ProductId ON StockBatches(ProductId)"),
        ("StockMovements", "CREATE INDEX IF NOT EXISTS IX_StockMovements_MovementType_MovementDate_ProductId ON StockMovements(MovementType, MovementDate, ProductId)"),
        ("PurchaseBills", "CREATE INDEX IF NOT EXISTS IX_PurchaseBills_BillDate ON PurchaseBills(BillDate)"),
        ("SalesBills", "CREATE INDEX IF NOT EXISTS IX_SalesBills_BillDate ON SalesBills(BillDate)"),
        ("PurchaseReturns", "CREATE INDEX IF NOT EXISTS IX_PurchaseReturns_ReturnDate ON PurchaseReturns(ReturnDate)"),
        ("SalesReturns", "CREATE INDEX IF NOT EXISTS IX_SalesReturns_ReturnDate ON SalesReturns(ReturnDate)"),
        ("PurchaseOrders", "CREATE INDEX IF NOT EXISTS IX_PurchaseOrders_OrderDate ON PurchaseOrders(OrderDate)"),
        ("PaymentEntries", "CREATE INDEX IF NOT EXISTS IX_PaymentEntries_PaymentDate ON PaymentEntries(PaymentDate)"),
        ("NotificationEntries", "CREATE INDEX IF NOT EXISTS IX_NotificationEntries_IsRead_TriggeredAtUtc ON NotificationEntries(IsRead, TriggeredAtUtc)"),
        ("BackupLogs", "CREATE INDEX IF NOT EXISTS IX_BackupLogs_CreatedAtUtc ON BackupLogs(CreatedAtUtc)"),
        ("CustomerLedgerEntries", "CREATE INDEX IF NOT EXISTS IX_CustomerLedgerEntries_EntryDate ON CustomerLedgerEntries(EntryDate)"),
        ("SupplierLedgerEntries", "CREATE INDEX IF NOT EXISTS IX_SupplierLedgerEntries_EntryDate ON SupplierLedgerEntries(EntryDate)")
    ];

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<SalesBill> SalesBills => Set<SalesBill>();
    public DbSet<SalesItem> SalesItems => Set<SalesItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
    public DbSet<PaymentEntry> PaymentEntries => Set<PaymentEntry>();
    public DbSet<SupplierLedgerEntry> SupplierLedgerEntries => Set<SupplierLedgerEntry>();
    public DbSet<CustomerLedgerEntry> CustomerLedgerEntries => Set<CustomerLedgerEntry>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<NotificationEntry> NotificationEntries => Set<NotificationEntry>();
    public DbSet<BackupLog> BackupLogs => Set<BackupLog>();

    // Authentication & User Management
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>().HasIndex(x => x.Name);
        modelBuilder.Entity<Supplier>().HasIndex(x => x.GstNo);

        modelBuilder.Entity<Product>().HasIndex(x => x.Name);

        modelBuilder.Entity<StockBatch>()
            .HasIndex(x => new { x.ProductId, x.BatchNo })
            .IsUnique();
        modelBuilder.Entity<StockBatch>().HasIndex(x => x.ExpiryDate);
        modelBuilder.Entity<StockBatch>().HasIndex(x => x.ProductId);
        modelBuilder.Entity<StockMovement>().HasIndex(x => new { x.MovementType, x.MovementDate, x.ProductId });

        modelBuilder.Entity<PurchaseBill>().HasIndex(x => x.BillNo).IsUnique();
        modelBuilder.Entity<PurchaseBill>().HasIndex(x => x.BillDate);
        modelBuilder.Entity<SalesBill>().HasIndex(x => x.BillNo).IsUnique();
        modelBuilder.Entity<SalesBill>().HasIndex(x => x.BillDate);
        modelBuilder.Entity<PurchaseReturn>().HasIndex(x => x.ReturnNo).IsUnique();
        modelBuilder.Entity<PurchaseReturn>().HasIndex(x => x.ReturnDate);
        modelBuilder.Entity<SalesReturn>().HasIndex(x => x.ReturnNo).IsUnique();
        modelBuilder.Entity<SalesReturn>().HasIndex(x => x.ReturnDate);
        modelBuilder.Entity<PurchaseOrder>().HasIndex(x => x.OrderNo).IsUnique();
        modelBuilder.Entity<PurchaseOrder>().HasIndex(x => x.OrderDate);
        modelBuilder.Entity<PaymentEntry>().HasIndex(x => x.PaymentDate);
        modelBuilder.Entity<NotificationEntry>().HasIndex(x => new { x.IsRead, x.TriggeredAtUtc });
        modelBuilder.Entity<BackupLog>().HasIndex(x => x.CreatedAtUtc);

        modelBuilder.Entity<CustomerLedgerEntry>().HasIndex(x => x.CustomerId);
        modelBuilder.Entity<CustomerLedgerEntry>().HasIndex(x => x.EntryDate);
        modelBuilder.Entity<SupplierLedgerEntry>().HasIndex(x => x.SupplierId);
        modelBuilder.Entity<SupplierLedgerEntry>().HasIndex(x => x.EntryDate);

        // User & Shop Configuration
        modelBuilder.Entity<Shop>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Shop>().HasIndex(x => x.Email);

        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.ShopId);
        modelBuilder.Entity<User>()
            .HasOne(x => x.Shop)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfile>().HasIndex(x => x.UserId).IsUnique();
        modelBuilder.Entity<UserProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.Profile)
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public static void EnsurePerformanceIndexes(ShopErpDbContext dbContext)
    {
        if (!dbContext.Database.IsSqlite())
        {
            return;
        }

        foreach (var pragma in SqlitePerformancePragmas)
        {
            dbContext.Database.ExecuteSqlRaw(pragma);
        }

        foreach (var (tableName, sql) in SqlitePerformanceIndexes)
        {
            if (!SqliteTableExists(dbContext, tableName))
            {
                continue;
            }

            dbContext.Database.ExecuteSqlRaw(sql);
        }
    }

    public static void EnsureSchemaCompatibility(ShopErpDbContext dbContext)
    {
        if (!dbContext.Database.IsSqlite())
        {
            return;
        }

        EnsureSqliteColumnExists(dbContext, "Doctors", "Mobile", "ALTER TABLE Doctors ADD COLUMN Mobile TEXT NOT NULL DEFAULT '';" );
        EnsureSqliteColumnExists(dbContext, "Doctors", "ClinicName", "ALTER TABLE Doctors ADD COLUMN ClinicName TEXT NOT NULL DEFAULT '';" );
        EnsureSqliteColumnExists(dbContext, "Doctors", "Address", "ALTER TABLE Doctors ADD COLUMN Address TEXT NOT NULL DEFAULT '';" );
        EnsureSqliteColumnExists(dbContext, "Doctors", "Email", "ALTER TABLE Doctors ADD COLUMN Email TEXT NOT NULL DEFAULT '';" );
        EnsureSqliteColumnExists(dbContext, "Doctors", "Status", "ALTER TABLE Doctors ADD COLUMN Status TEXT NOT NULL DEFAULT 'Active';" );
        EnsureSqliteColumnExists(dbContext, "Doctors", "CreatedAtUtc", "ALTER TABLE Doctors ADD COLUMN CreatedAtUtc TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP);" );

        EnsureSqliteColumnExists(dbContext, "Products", "GstPercent", "ALTER TABLE Products ADD COLUMN GstPercent REAL NOT NULL DEFAULT 0;" );
        EnsureSqliteColumnExists(dbContext, "Products", "Status", "ALTER TABLE Products ADD COLUMN Status TEXT NOT NULL DEFAULT 'Active';" );
    }

    private static bool SqliteTableExists(ShopErpDbContext dbContext, string tableName)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = command.ExecuteScalar();
            return result is not null && Convert.ToInt64(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static void EnsureSqliteColumnExists(ShopErpDbContext dbContext, string tableName, string columnName, string addColumnSql)
    {
        if (!SqliteTableExists(dbContext, tableName) || SqliteColumnExists(dbContext, tableName, columnName))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw(addColumnSql);
    }

    private static bool SqliteColumnExists(ShopErpDbContext dbContext, string tableName, string columnName)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({tableName});";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader[1]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }
}


