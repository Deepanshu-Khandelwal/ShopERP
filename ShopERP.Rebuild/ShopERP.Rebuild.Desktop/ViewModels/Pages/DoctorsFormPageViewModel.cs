using ShopERP.Rebuild.Desktop.Services;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public sealed class DoctorsFormPageViewModel(DoctorCrudService doctorService)
    : DoctorsPageViewModel(doctorService);