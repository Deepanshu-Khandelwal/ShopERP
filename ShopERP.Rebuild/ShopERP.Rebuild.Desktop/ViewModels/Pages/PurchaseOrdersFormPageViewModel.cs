using ShopERP.Backend.Services;
using ShopERP.Rebuild.Desktop.Services;
using ShopERP.Rebuild.Desktop.ViewModels;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class PurchaseOrdersFormPageViewModel(
    BackendDbContext dbContext,
    IPurchaseOrderService purchaseOrderService,
    PurchaseOrderDraftStore draftStore,
    IShellNavigationService navigationService)
    : PurchaseOrdersPageViewModel(dbContext, purchaseOrderService, draftStore, navigationService), IPageActivationAware
{
    public async Task OnNavigatedToAsync()
    {
        ResetEditorFields();
        await LoadAsync();

        if (DraftStore.Current is not null)
        {
            ApplyDraft(DraftStore.Current);
            Status = "Purchase order draft loaded";
        }
    }
}
