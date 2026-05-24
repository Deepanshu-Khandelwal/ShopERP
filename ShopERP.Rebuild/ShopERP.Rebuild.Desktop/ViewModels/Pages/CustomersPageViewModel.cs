using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShopERP.Rebuild.Core.Domain.Entities;
using ShopERP.Rebuild.Desktop.Services;
using System.Collections.ObjectModel;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class CustomersPageViewModel(CustomerCrudService customerService) : PageViewModelBase("Customers")
{
    [ObservableProperty]
    private ObservableCollection<Customer> _items = new();

    [ObservableProperty]
    private Customer? _selectedItem;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _medicalHistory = string.Empty;

    [ObservableProperty]
    private string _status = "Ready";

    public async Task LoadAsync()
    {
        var rows = await customerService.ListAsync();
        Items = new ObservableCollection<Customer>(rows);
        Status = $"Loaded {Items.Count} customers";
    }

    partial void OnSelectedItemChanged(Customer? value)
    {
        if (value is null)
        {
            return;
        }

        Name = value.Name;
        Phone = value.Phone;
        Email = value.Email;
        Address = value.Address;
        MedicalHistory = value.MedicalHistory;
    }

    [RelayCommand]
    private void NewForm()
    {
        SelectedItem = null;
        Name = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        MedicalHistory = string.Empty;
        Status = "New patient";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Status = "Name is required.";
            return;
        }

        var model = SelectedItem ?? new Customer();
        model.Name = Name.Trim();
        model.Phone = Phone.Trim();
        model.Email = Email.Trim();
        model.Address = Address.Trim();
        model.MedicalHistory = MedicalHistory.Trim();

        await customerService.SaveAsync(model);
        await LoadAsync();
        Status = "Customer saved";
    }

    [RelayCommand]
    private void Edit(Customer? item)
    {
        if (item is null) return;
        SelectedItem = item;
    }

    [RelayCommand]
    private async Task DeleteAsync(Customer? item)
    {
        if (item is null) return;
        await customerService.DeleteAsync(item);
        await LoadAsync();
        Status = "Customer deleted";
    }
}
