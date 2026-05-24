using ShopERP.Backend.Services;
using ShopERP.Rebuild.Desktop.Services;
using ShopERP.Rebuild.Desktop.ViewModels;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class PaymentsFormPageViewModel(
    BackendDbContext dbContext,
    IPaymentService paymentService,
    PaymentDraftStore draftStore,
    IShellNavigationService navigationService)
    : PaymentsPageViewModel(dbContext, paymentService, draftStore, navigationService), IPageActivationAware
{
    public async Task OnNavigatedToAsync()
    {
        ResetEditorFields();
        await LoadAsync();

        if (DraftStore.Current is not null)
        {
            ApplyDraft(DraftStore.Current);
            Status = "Payment draft loaded";
        }
    }
}
