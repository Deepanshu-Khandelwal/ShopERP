using ShopERP.Backend.Services;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class ReturnsFormPageViewModel(
    BackendDbContext dbContext,
    IReturnService returnService)
    : ReturnsPageViewModel(dbContext, returnService);
