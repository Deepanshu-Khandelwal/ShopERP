using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Services;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class BillingFormPageViewModel(
    BackendDbContext dbContext,
    ISalesService salesService,
    IPurchaseService purchaseService,
    IInvoiceService invoiceService)
    : BillingPageViewModel(dbContext, salesService, purchaseService, invoiceService);
