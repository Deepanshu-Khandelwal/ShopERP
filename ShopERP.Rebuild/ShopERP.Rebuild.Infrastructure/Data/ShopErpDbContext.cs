using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using ShopERP.Rebuild.Core.Configuration;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Infrastructure.Security;

namespace ShopERP.Rebuild.Infrastructure.Data;

public sealed class ShopErpDbContext : DbContext
{
    private readonly SyncOptions _syncOptions;

    public ShopErpDbContext(
        DbContextOptions<ShopErpDbContext> options,
        IOptions<SyncOptions>? syncOptions = null) : base(options)
    {
        _syncOptions = syncOptions?.Value ?? new SyncOptions();
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();
    public DbSet<SyncState> SyncStates => Set<SyncState>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var protector = new AesProtector(_syncOptions.AesKey);
        var encryptConverter = new ValueConverter<string, string>(
            value => string.IsNullOrWhiteSpace(value) ? value : protector.Protect(value),
            value => string.IsNullOrWhiteSpace(value) ? value : protector.Unprotect(value));

        modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.BatchNo);
        modelBuilder.Entity<Sale>().HasIndex(s => s.InvoiceNo).IsUnique();
        modelBuilder.Entity<Purchase>().HasIndex(p => p.BillNo).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(u => u.Username).IsUnique();

        modelBuilder.Entity<Customer>().Property(x => x.Phone).HasConversion(encryptConverter);
        modelBuilder.Entity<Customer>().Property(x => x.Email).HasConversion(encryptConverter);
        modelBuilder.Entity<Customer>().Property(x => x.MedicalHistory).HasConversion(encryptConverter);

        modelBuilder.Entity<Sale>()
            .HasMany(s => s.Lines)
            .WithOne(l => l.Sale)
            .HasForeignKey(l => l.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SaleLine>()
            .HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Purchase>()
            .HasMany(p => p.Lines)
            .WithOne(l => l.Purchase)
            .HasForeignKey(l => l.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SyncState>().HasData(new SyncState
        {
            Id = 1,
            LastSyncedUtc = DateTime.MinValue,
            LastError = string.Empty
        });
    }
}
