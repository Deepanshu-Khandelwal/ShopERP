using ShopERP.Rebuild.Desktop.Services;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class PurchasesFormPageViewModel(PurchasesCrudService purchasesService)
    : PurchasesPageViewModel(purchasesService);