using ShopERP.Rebuild.Desktop.Services;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class ProductsFormPageViewModel(ProductMasterService productService)
    : ProductsPageViewModel(productService);