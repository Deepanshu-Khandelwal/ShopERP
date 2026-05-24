using ShopERP.Rebuild.Desktop.Services;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class CustomersFormPageViewModel(CustomerCrudService customerService)
    : CustomersPageViewModel(customerService);