using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Rebuild.Desktop.Services;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class DoctorsPageViewModel(DoctorCrudService doctorService) : PageViewModelBase("Doctors")
{
    [ObservableProperty] private ObservableCollection<Doctor> _items = new();
    [ObservableProperty] private ObservableCollection<Doctor> _filteredItems = new();
    [ObservableProperty] private Doctor? _selectedItem;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusFilter = "All";

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _registrationNo = string.Empty;
    [ObservableProperty] private string _specialization = string.Empty;
    [ObservableProperty] private string _degree = string.Empty;
    [ObservableProperty] private string _mobile = string.Empty;
    [ObservableProperty] private string _clinicName = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _visitDetails = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _doctorStatus = "Active";
    [ObservableProperty] private string _status = "Ready";

    public IReadOnlyList<string> StatusOptions { get; } = ["All", "Active", "Inactive"];
    public IReadOnlyList<string> DoctorStatusOptions { get; } = ["Active", "Inactive"];
    public int RecordCount => FilteredItems.Count;

    public async Task LoadAsync()
    {
        var rows = await doctorService.ListAsync();
        Items = new ObservableCollection<Doctor>(rows);
        ApplyFilter();
        Status = $"Loaded {Items.Count} practitioners";
    }

    partial void OnSelectedItemChanged(Doctor? value)
    {
        if (value is null)
        {
            return;
        }

        Name = value.Name;
        RegistrationNo = value.DoctorId;
        Specialization = value.Specialty;
        Degree = value.Degree;
        Mobile = value.Mobile;
        ClinicName = value.ClinicName;
        Address = value.Address;
        VisitDetails = value.ClinicVisitDetails;
        Email = value.Email;
        DoctorStatus = string.IsNullOrWhiteSpace(value.Status) ? "Active" : value.Status;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnStatusFilterChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void NewForm()
    {
        SelectedItem = null;
        Name = string.Empty;
        RegistrationNo = string.Empty;
        Specialization = string.Empty;
        Degree = string.Empty;
        Mobile = string.Empty;
        ClinicName = string.Empty;
        Address = string.Empty;
        VisitDetails = string.Empty;
        Email = string.Empty;
        DoctorStatus = "Active";
        Status = "New practitioner registration";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Status = "Practitioner name is required.";
            return;
        }

        var model = SelectedItem ?? new Doctor();
        model.Name = Name.Trim();
        model.DoctorId = RegistrationNo.Trim();
        model.Specialty = Specialization.Trim();
        model.Degree = Degree.Trim();
        model.Mobile = Mobile.Trim();
        model.ClinicName = ClinicName.Trim();
        model.Address = Address.Trim();
        model.ClinicVisitDetails = VisitDetails.Trim();
        model.Email = Email.Trim();
        model.Status = string.IsNullOrWhiteSpace(DoctorStatus) ? "Active" : DoctorStatus.Trim();

        await doctorService.SaveAsync(model);
        await LoadAsync();
        Status = "Practitioner record establishment complete";
    }

    [RelayCommand]
    private void Edit(Doctor? item)
    {
        if (item is null)
        {
            return;
        }

        SelectedItem = item;
    }

    [RelayCommand]
    private async Task DeleteAsync(Doctor? item)
    {
        if (item is null)
        {
            return;
        }

        await doctorService.DeleteAsync(item);
        await LoadAsync();
        Status = "Doctor deleted";
    }

    private void ApplyFilter()
    {
        IEnumerable<Doctor> query = Items;

        if (!string.Equals(StatusFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => string.Equals(x.Status, StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        var keyword = SearchText.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.Specialty.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.Mobile.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.DoctorId.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        FilteredItems = new ObservableCollection<Doctor>(query.OrderBy(x => x.Name));
        OnPropertyChanged(nameof(RecordCount));
    }
}