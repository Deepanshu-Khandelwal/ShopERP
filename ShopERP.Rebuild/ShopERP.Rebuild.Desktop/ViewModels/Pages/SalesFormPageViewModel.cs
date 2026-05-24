using ShopERP.Rebuild.Desktop.Services;
using ShopERP.Backend.Data;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class SalesFormPageViewModel(
    SalesCrudService salesService,
    ProductCrudService productService,
    ShopErpDbContext backendDbContext)
    : SalesPageViewModel(salesService, productService, backendDbContext);