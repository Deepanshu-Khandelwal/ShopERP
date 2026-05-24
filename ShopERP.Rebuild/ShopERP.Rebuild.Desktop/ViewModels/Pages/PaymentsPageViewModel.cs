using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;
using ShopERP.Backend.Services;
using ShopERP.Rebuild.Desktop.Services;
using System.Collections.ObjectModel;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class PaymentsPageViewModel(
    BackendDbContext dbContext,
    IPaymentService paymentService,
    PaymentDraftStore draftStore,
    IShellNavigationService navigationService) : PageViewModelBase("Payments")
{
    public sealed class PartyOption
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private ObservableCollection<PartyOption> _partyOptions = new();
    [ObservableProperty] private ObservableCollection<PaymentEntry> _payments = new();
    [ObservableProperty] private PaymentPartyType _selectedPartyType = PaymentPartyType.Customer;
    [ObservableProperty] private int? _selectedPartyId;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private DateTime _paymentDate = DateTime.Today;
    [ObservableProperty] private string _referenceNo = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private string _selectedPartyName = string.Empty;
    [ObservableProperty] private decimal _currentBalance;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private string _status = "Ready";

    protected PaymentDraftStore DraftStore { get; } = draftStore;
    protected IShellNavigationService NavigationService { get; } = navigationService;

    public Array PartyTypes => Enum.GetValues(typeof(PaymentPartyType));
    public string SaveActionLabel => IsEditMode ? "Update Payment" : "Record Payment";

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(SaveActionLabel));
    }

    public async Task LoadAsync()
    {
        Customers = new ObservableCollection<Customer>(await dbContext.Customers.OrderBy(x => x.Name).ToListAsync(default));
        Suppliers = new ObservableCollection<Supplier>(await dbContext.Suppliers.OrderBy(x => x.Name).ToListAsync(default));
        Payments = new ObservableCollection<PaymentEntry>(await dbContext.PaymentEntries.OrderByDescending(x => x.PaymentDate).Take(200).ToListAsync(default));
        RefreshPartyOptions();
        await RefreshPartyBalanceAsync();
        Status = "Payments loaded";
    }

    partial void OnSelectedPartyTypeChanged(PaymentPartyType value)
    {
        RefreshPartyOptions();
        _ = RefreshPartyBalanceAsync();
    }

    partial void OnSelectedPartyIdChanged(int? value)
    {
        _ = RefreshPartyBalanceAsync();
    }

    [RelayCommand]
    private async Task RecordPaymentAsync()
    {
        if (!SelectedPartyId.HasValue || SelectedPartyId.Value <= 0 || Amount <= 0)
        {
            Status = "Select party and amount.";
            return;
        }

        if (DraftStore.Current is { } draft)
        {
            var existingPayment = await dbContext.PaymentEntries
                .FirstOrDefaultAsync(x => x.Id == draft.PaymentId, default);

            if (existingPayment is null)
            {
                Status = "Payment not found for update.";
                DraftStore.Clear();
                IsEditMode = false;
                return;
            }

            if (draft.OriginalPartyType == PaymentPartyType.Customer)
            {
                var oldEntries = await dbContext.CustomerLedgerEntries
                    .Where(x => x.CustomerId == draft.OriginalPartyId && x.VoucherNo == draft.OriginalReferenceNo)
                    .ToListAsync(default);
                dbContext.CustomerLedgerEntries.RemoveRange(oldEntries);
            }
            else
            {
                var oldEntries = await dbContext.SupplierLedgerEntries
                    .Where(x => x.SupplierId == draft.OriginalPartyId && x.VoucherNo == draft.OriginalReferenceNo)
                    .ToListAsync(default);
                dbContext.SupplierLedgerEntries.RemoveRange(oldEntries);
            }

            existingPayment.PartyType = SelectedPartyType;
            existingPayment.PartyId = SelectedPartyId.Value;
            existingPayment.Amount = Amount;
            existingPayment.PaymentDate = PaymentDate;
            existingPayment.ReferenceNo = string.IsNullOrWhiteSpace(ReferenceNo) ? $"PAY-{DateTime.Now:yyyyMMdd-HHmmss}" : ReferenceNo.Trim();
            existingPayment.Notes = Notes.Trim();

            if (existingPayment.PartyType == PaymentPartyType.Customer)
            {
                var balance = await dbContext.CustomerLedgerEntries
                    .Where(x => x.CustomerId == existingPayment.PartyId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Balance)
                    .FirstOrDefaultAsync(default);

                dbContext.CustomerLedgerEntries.Add(new CustomerLedgerEntry
                {
                    CustomerId = existingPayment.PartyId,
                    EntryDate = existingPayment.PaymentDate,
                    VoucherNo = existingPayment.ReferenceNo,
                    BillAmount = 0,
                    PaymentAmount = existingPayment.Amount,
                    Balance = balance - existingPayment.Amount,
                    Narration = "Customer payment"
                });
            }
            else
            {
                var balance = await dbContext.SupplierLedgerEntries
                    .Where(x => x.SupplierId == existingPayment.PartyId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Balance)
                    .FirstOrDefaultAsync(default);

                dbContext.SupplierLedgerEntries.Add(new SupplierLedgerEntry
                {
                    SupplierId = existingPayment.PartyId,
                    EntryDate = existingPayment.PaymentDate,
                    VoucherNo = existingPayment.ReferenceNo,
                    PurchaseAmount = 0,
                    PaymentAmount = existingPayment.Amount,
                    Balance = balance - existingPayment.Amount,
                    Narration = "Supplier payment"
                });
            }

            await dbContext.SaveChangesAsync(default);
            await LoadAsync();
            DraftStore.Clear();
            IsEditMode = false;
            ResetEditorFields();
            Status = "Payment updated";
            return;
        }

        var request = new PaymentCreateRequest
        {
            PartyType = SelectedPartyType,
            PartyId = SelectedPartyId.Value,
            Amount = Amount,
            PaymentDate = PaymentDate,
            ReferenceNo = string.IsNullOrWhiteSpace(ReferenceNo) ? $"PAY-{DateTime.Now:yyyyMMdd-HHmmss}" : ReferenceNo.Trim(),
            Notes = Notes.Trim()
        };

        await paymentService.RecordPaymentAsync(request, default);
        await LoadAsync();
        DraftStore.Clear();
        IsEditMode = false;
        Amount = 0;
        Notes = string.Empty;
        Status = "Payment recorded";
    }

    private void RefreshPartyOptions()
    {
        if (SelectedPartyType == PaymentPartyType.Customer)
        {
            PartyOptions = new ObservableCollection<PartyOption>(
                Customers.Select(x => new PartyOption { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name));
        }
        else
        {
            PartyOptions = new ObservableCollection<PartyOption>(
                Suppliers.Select(x => new PartyOption { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name));
        }

        if (SelectedPartyId.HasValue && PartyOptions.All(x => x.Id != SelectedPartyId.Value))
        {
            SelectedPartyId = null;
        }
    }

    private async Task RefreshPartyBalanceAsync()
    {
        if (!SelectedPartyId.HasValue || SelectedPartyId.Value <= 0)
        {
            SelectedPartyName = string.Empty;
            CurrentBalance = 0;
            return;
        }

        if (SelectedPartyType == PaymentPartyType.Customer)
        {
            SelectedPartyName = Customers.FirstOrDefault(x => x.Id == SelectedPartyId.Value)?.Name ?? string.Empty;
            CurrentBalance = await dbContext.CustomerLedgerEntries
                .Where(x => x.CustomerId == SelectedPartyId.Value)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Balance)
                .FirstOrDefaultAsync(default);
            return;
        }

        SelectedPartyName = Suppliers.FirstOrDefault(x => x.Id == SelectedPartyId.Value)?.Name ?? string.Empty;
        CurrentBalance = await dbContext.SupplierLedgerEntries
            .Where(x => x.SupplierId == SelectedPartyId.Value)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync(default);
    }

    protected void ResetEditorFields()
    {
        IsEditMode = false;
        SelectedPartyType = PaymentPartyType.Customer;
        SelectedPartyId = null;
        Amount = 0;
        PaymentDate = DateTime.Today;
        ReferenceNo = string.Empty;
        Notes = string.Empty;
        SelectedPartyName = string.Empty;
        CurrentBalance = 0;
    }

    protected void ApplyDraft(PaymentDraft draft)
    {
        IsEditMode = true;
        SelectedPartyType = draft.PartyType;
        SelectedPartyId = draft.PartyId;
        Amount = draft.Amount;
        PaymentDate = draft.PaymentDate;
        ReferenceNo = draft.ReferenceNo;
        Notes = draft.Notes;
    }

    [RelayCommand]
    private async Task EditAsync(PaymentEntry? item)
    {
        if (item is null) return;

        DraftStore.SetFrom(item);
        IsEditMode = true;
        Status = "Opening payment draft";
        await NavigationService.NavigateAsync("PaymentsForm");
    }

    [RelayCommand]
    private async Task DeleteAsync(PaymentEntry? item)
    {
        if (item is null) return;

        if (item.PartyType == PaymentPartyType.Customer)
        {
            var customerLedgerEntries = await dbContext.CustomerLedgerEntries
                .Where(x => x.CustomerId == item.PartyId && x.VoucherNo == item.ReferenceNo)
                .ToListAsync(default);
            dbContext.CustomerLedgerEntries.RemoveRange(customerLedgerEntries);
        }
        else
        {
            var supplierLedgerEntries = await dbContext.SupplierLedgerEntries
                .Where(x => x.SupplierId == item.PartyId && x.VoucherNo == item.ReferenceNo)
                .ToListAsync(default);
            dbContext.SupplierLedgerEntries.RemoveRange(supplierLedgerEntries);
        }

        dbContext.PaymentEntries.Remove(item);
        await dbContext.SaveChangesAsync(default);
        await LoadAsync();
        Status = "Payment deleted";
    }
}
