using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Contracts.Dtos;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public interface IGstService
{
    (decimal cgst, decimal sgst, decimal totalGst) Calculate(decimal taxableAmount, decimal gstPercent);
}

public interface IStockService
{
    Task IncreaseAsync(int productId, string batchNo, DateTime expiryDate, int quantity, decimal purchaseRate, decimal saleRate, decimal mrp, StockMovementType movementType, string referenceNo, CancellationToken ct);
    Task DecreaseAsync(int stockBatchId, int quantity, string referenceNo, CancellationToken ct);
}

public interface IPurchaseService
{
    Task<PurchaseBill> CreateAsync(PurchaseBillCreateRequest request, CancellationToken ct);
}

public interface IPurchaseCalculationService
{
    PurchaseLineComputation BuildPurchaseItem(BillLineDto item);
    void ApplyBillTotals(PurchaseBill bill, decimal lessDiscount, decimal roundOff, decimal subtotal, decimal cgst, decimal sgst);
}

public interface ISupplierService
{
    Task<Supplier> GetSupplierOrThrowAsync(int supplierId, CancellationToken ct);
}

public interface ISupplierLedgerService
{
    Task AddPurchaseBillEntryAsync(PurchaseBill bill, CancellationToken ct);
}

public interface ISalesService
{
    Task<SalesBill> CreateAsync(SalesBillCreateRequest request, CancellationToken ct);
}

public interface ISalesCalculationService
{
    SalesItem BuildSalesItem(StockBatch batch, BillLineDto requestItem);
    void ApplyBillTotals(SalesBill bill, decimal requestedPaidAmount);
}

public interface ICustomerService
{
    Task<int?> ResolveOrCreateCustomerIdAsync(int? customerId, string? customerName, string? customerMobile, string? customerAddress, CancellationToken ct);
    Task<int?> ResolveOrCreateDoctorIdAsync(int? doctorId, string? doctorName, CancellationToken ct);
}

public interface ILedgerService
{
    Task AddCustomerSalesEntryAsync(SalesBill bill, CancellationToken ct);
}

public interface IReturnService
{
    Task<PurchaseReturn> CreatePurchaseReturnAsync(ReturnCreateRequest request, CancellationToken ct);
    Task<SalesReturn> CreateSalesReturnAsync(ReturnCreateRequest request, CancellationToken ct);
}

public interface IPaymentService
{
    Task<PaymentEntry> RecordPaymentAsync(PaymentCreateRequest request, CancellationToken ct);
}

public interface IReportService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken ct);
}

public interface INotificationService
{
    Task GenerateSystemNotificationsAsync(CancellationToken ct);
}

public interface IInvoiceService
{
    Task<byte[]> CreateSalesInvoicePdfAsync(int salesBillId, CancellationToken ct);
}

public interface IBackupService
{
    Task<BackupLog> RunDailyBackupAsync(CancellationToken ct);
    Task<bool> RestoreAsync(string backupFilePath, CancellationToken ct);
}

public interface IPurchaseOrderService
{
    Task<PurchaseOrder> CreateAsync(PurchaseOrderCreateRequest request, CancellationToken ct);
    Task<PurchaseOrder> MarkSentAsync(int orderId, CancellationToken ct);
    Task<PurchaseBill> ConvertToPurchaseBillAsync(int orderId, string billNo, DateTime billDate, CancellationToken ct);
}
public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken ct);
    Task<UserCreateResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
    Task<ShopCreateResponse> CreateShopAsync(CreateShopRequest request, CancellationToken ct);
    Task<UserDto?> GetUserAsync(int userId, CancellationToken ct);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IProfileService
{
    Task<UserProfileDto?> GetProfileAsync(int userId, CancellationToken ct);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct);
    Task<UserDto> GetUserWithProfileAsync(int userId, CancellationToken ct);
}
public interface IMySqlSyncService
{
    Task<MySqlSyncResult> SyncToMySqlAsync(bool fullSync, CancellationToken ct);
}

public sealed record MySqlSyncResult(
    bool Success,
    string Message,
    string SyncMode,
    string TargetDatabase,
    int TablesSynced,
    int RowsSynced,
    DateTime? PreviousCheckpointUtc,
    DateTime CompletedAtUtc);


