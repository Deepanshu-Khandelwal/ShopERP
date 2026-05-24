using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Infrastructure.Backup;
using ShopERP.Backend.Infrastructure.Jobs;
using ShopERP.Backend.Infrastructure.Pdf;
using ShopERP.Backend.Infrastructure.Sync;
using ShopERP.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<ShopErpDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ShopERP")));

builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection("Backup"));
builder.Services.Configure<MySqlSyncOptions>(builder.Configuration.GetSection("MySqlSync"));

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IGstService, GstService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ISalesCalculationService, SalesCalculationService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IPurchaseCalculationService, PurchaseCalculationService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISupplierLedgerService, SupplierLedgerService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IMySqlSyncService, MySqlSyncService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShopErpDbContext>();
    dbContext.Database.EnsureCreated();
    ShopErpDbContext.EnsureSchemaCompatibility(dbContext);
    ShopErpDbContext.EnsurePerformanceIndexes(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseHangfireDashboard("/jobs");
ScheduledJobs.Configure();

app.MapControllers();
app.Run();



