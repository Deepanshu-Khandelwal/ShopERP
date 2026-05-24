using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ShopERP.Backend.Contracts.Dtos;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;
using ShopERP.Backend.Services;
using ShopERP.Rebuild.Desktop.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.ViewModels.Pages;

public partial class BillingPageViewModel(
    BackendDbContext dbContext,
    ISalesService salesService,
    IPurchaseService purchaseService,
    IInvoiceService invoiceService) : PageViewModelBase("Billing")
{
    private int _lastSalesBillId;

    [ObservableProperty] private ObservableCollection<Product> _products = new();
    [ObservableProperty] private ObservableCollection<Product> _filteredProducts = new();
    [ObservableProperty] private ObservableCollection<StockBatch> _batches = new();
    [ObservableProperty] private ObservableCollection<StockBatch> _filteredBatches = new();
    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private ObservableCollection<Doctor> _doctors = new();
    [ObservableProperty] private ObservableCollection<Doctor> _filteredDoctors = new();
    [ObservableProperty] private ObservableCollection<BackendBillLineRow> _lines = new();
    [ObservableProperty] private BackendBillLineRow? _selectedLine;

    [ObservableProperty] private string _salesBillNo = string.Empty;
    [ObservableProperty] private string _prescriptionNo = string.Empty;
    [ObservableProperty] private string _purchaseBillNo = string.Empty;
    [ObservableProperty] private DateTime _billDate = DateTime.Today;
    [ObservableProperty] private int? _selectedCustomerId;
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerMobile = string.Empty;
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private int? _selectedSupplierId;
    [ObservableProperty] private int? _selectedDoctorId;
    [ObservableProperty] private string _doctorSearchText = string.Empty;
    [ObservableProperty] private string _productSearchText = string.Empty;
    [ObservableProperty] private string _quickAddDoctorName = string.Empty;
    [ObservableProperty] private string _quickAddDoctorMobile = string.Empty;
    [ObservableProperty] private string _quickAddDoctorSpecialization = string.Empty;
    [ObservableProperty] private string _quickAddDoctorRegistrationNo = string.Empty;
    [ObservableProperty] private int _selectedProductId;
    [ObservableProperty] private int? _selectedBatchId;
    [ObservableProperty] private int _lineQty = 1;
    [ObservableProperty] private decimal _lineRate;
    [ObservableProperty] private decimal _lineMrp;
    [ObservableProperty] private decimal _lineDiscountPercent;
    [ObservableProperty] private decimal _lineGstPercent = 12;
    [ObservableProperty] private decimal _paidAmount;
    [ObservableProperty] private decimal _salesDiscountPercent;
    [ObservableProperty] private decimal _purchaseLessDiscount;
    [ObservableProperty] private decimal _purchaseRoundOff;
    [ObservableProperty] private PaymentType _selectedPaymentType = PaymentType.Credit;
    [ObservableProperty] private string _status = "Ready";
    [ObservableProperty] private string _invoicePreview = "No invoice preview yet.";

    public Array PaymentTypes => Enum.GetValues(typeof(PaymentType));
    public decimal NetAmount => Math.Round(Lines.Sum(x => x.Amount), 2);
    public decimal LineDiscountAmount => Math.Round(Lines.Sum(x => x.Amount * Math.Max(0, x.DiscountPercent) / 100m), 2);
    public decimal TaxableAmount => Math.Round(NetAmount - LineDiscountAmount, 2);
    public decimal GstAmount => Math.Round(Lines.Sum(x => (x.Amount - (x.Amount * Math.Max(0, x.DiscountPercent) / 100m)) * Math.Max(0, x.GstPercent) / 100m), 2);
    public decimal CgstAmount => Math.Round(GstAmount / 2m, 2);
    public decimal SgstAmount => Math.Round(GstAmount / 2m, 2);
    public decimal EstimatedGrandTotal => Math.Round(TaxableAmount + GstAmount, 2);
    public bool ShowQuickAddDoctor => FilteredDoctors.Count == 0 && !string.IsNullOrWhiteSpace(DoctorSearchText);

    partial void OnLinesChanged(ObservableCollection<BackendBillLineRow> value)
    {
        value.CollectionChanged += OnLinesCollectionChanged;
        RecalculateSummaries();
    }

    public async Task LoadAsync()
    {
        Lines.CollectionChanged -= OnLinesCollectionChanged;
        Products = new ObservableCollection<Product>(await dbContext.Products.OrderBy(x => x.Name).ToListAsync(default));
        FilteredProducts = new ObservableCollection<Product>(Products);
        Batches = new ObservableCollection<StockBatch>(await dbContext.StockBatches.Include(x => x.Product).Where(x => x.Quantity > 0).OrderBy(x => x.ExpiryDate).ToListAsync(default));
        FilteredBatches = new ObservableCollection<StockBatch>(Batches);
        Customers = new ObservableCollection<Customer>(await dbContext.Customers.OrderBy(x => x.Name).ToListAsync(default));
        Suppliers = new ObservableCollection<Supplier>(await dbContext.Suppliers.OrderBy(x => x.Name).ToListAsync(default));
        Doctors = new ObservableCollection<Doctor>(await dbContext.Doctors.OrderBy(x => x.Name).ToListAsync(default));
        ApplyDoctorFilter();
        Lines.CollectionChanged += OnLinesCollectionChanged;

        if (string.IsNullOrWhiteSpace(SalesBillNo))
        {
            SalesBillNo = $"SB-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        if (string.IsNullOrWhiteSpace(PurchaseBillNo))
        {
            PurchaseBillNo = $"PB-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        FilterBatchesForProduct(SelectedProductId);
        RecalculateSummaries();
        Status = "Billing data loaded";
    }

    partial void OnSelectedProductIdChanged(int value)
    {
        FilterBatchesForProduct(value);

        var selected = Products.FirstOrDefault(x => x.Id == value);
        if (selected is not null)
        {
            ProductSearchText = selected.Name;
        }

        var batch = Batches.FirstOrDefault(x => x.ProductId == value && x.Quantity > 0);
        if (batch is null)
        {
            return;
        }

        SelectedBatchId = batch.Id;
        LineRate = batch.SaleRate;
        LineMrp = batch.Mrp;
    }

    partial void OnSelectedBatchIdChanged(int? value)
    {
        if (value is null)
        {
            return;
        }

        var batch = Batches.FirstOrDefault(x => x.Id == value.Value);
        if (batch is null)
        {
            return;
        }

        SelectedProductId = batch.ProductId;
        LineRate = batch.SaleRate;
        LineMrp = batch.Mrp;

        if (LineQty > batch.Quantity)
        {
            LineQty = batch.Quantity;
        }
    }

    partial void OnProductSearchTextChanged(string value)
    {
        ApplyProductFilter(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var matchedBatch = Batches
            .Where(x => x.Quantity > 0 && x.BatchNo.Contains(value, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ExpiryDate)
            .FirstOrDefault();

        if (matchedBatch is null)
        {
            return;
        }

        SelectedProductId = matchedBatch.ProductId;
        SelectedBatchId = matchedBatch.Id;
    }

    partial void OnDoctorSearchTextChanged(string value)
    {
        ApplyDoctorFilter();
    }

    partial void OnSelectedDoctorIdChanged(int? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        var selectedDoctor = Doctors.FirstOrDefault(x => x.Id == value.Value);
        if (selectedDoctor is not null)
        {
            DoctorSearchText = selectedDoctor.Name;
        }
    }

    partial void OnFilteredDoctorsChanged(ObservableCollection<Doctor> value)
    {
        OnPropertyChanged(nameof(ShowQuickAddDoctor));
    }

    [RelayCommand]
    private void AddLine()
    {
        if (SelectedProductId <= 0)
        {
            Status = "Select a product.";
            return;
        }

        var product = Products.FirstOrDefault(x => x.Id == SelectedProductId);
        var batch = SelectedBatchId.HasValue ? Batches.FirstOrDefault(x => x.Id == SelectedBatchId.Value) : null;

        if (batch is null)
        {
            Status = "Select a stock batch for billing.";
            return;
        }

        if (batch.ProductId != SelectedProductId)
        {
            Status = "Batch and product do not match.";
            return;
        }

        if (LineQty <= 0)
        {
            Status = "Quantity must be greater than zero.";
            return;
        }

        if (LineQty > batch.Quantity)
        {
            Status = $"Only {batch.Quantity} units available in selected batch.";
            return;
        }

        if (LineRate <= 0)
        {
            Status = "Rate must be greater than zero.";
            return;
        }

        Lines.Add(new BackendBillLineRow
        {
            ProductId = SelectedProductId,
            ProductName = product?.Name ?? string.Empty,
            StockBatchId = SelectedBatchId,
            BatchNo = batch?.BatchNo ?? string.Empty,
            ExpiryDate = batch?.ExpiryDate,
            Quantity = Math.Max(1, LineQty),
            Rate = Math.Max(0, LineRate),
            Mrp = Math.Max(0, LineMrp),
            DiscountPercent = Math.Max(0, LineDiscountPercent),
            GstPercent = Math.Max(0, LineGstPercent)
        });

        RecalculateSummaries();
        Status = "Line added";
    }

    [RelayCommand]
    private void RemoveLine()
    {
        if (SelectedLine is null)
        {
            return;
        }

        Lines.Remove(SelectedLine);
        RecalculateSummaries();
    }

    [RelayCommand]
    private async Task CreateSalesBillAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            Status = "Customer name is required for sales billing.";
            return;
        }

        if (Lines.Count == 0)
        {
            Status = "Add at least one line.";
            return;
        }

        var request = new SalesBillCreateRequest
        {
            BillNo = SalesBillNo,
            BillDate = BillDate,
            PaymentType = SelectedPaymentType,
            CustomerId = null,
            CustomerName = CustomerName.Trim(),
            CustomerMobile = CustomerMobile.Trim(),
            CustomerAddress = CustomerAddress.Trim(),
            DoctorId = SelectedDoctorId,
            DoctorName = string.IsNullOrWhiteSpace(DoctorSearchText) ? null : DoctorSearchText.Trim(),
            DiscountPercent = Math.Max(0, SalesDiscountPercent),
            PaidAmount = Math.Max(0, PaidAmount),
            Items = Lines.Select(x => new BillLineDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                BatchNo = x.BatchNo,
                StockBatchId = x.StockBatchId,
                ExpiryDate = x.ExpiryDate,
                Quantity = x.Quantity,
                Mrp = x.Mrp,
                Rate = x.Rate,
                DiscountPercent = x.DiscountPercent,
                GstPercent = x.GstPercent,
                Amount = x.Amount
            }).ToList()
        };

        var bill = await salesService.CreateAsync(request, default);
        _lastSalesBillId = bill.Id;
        Status = $"Sales bill created. Grand={bill.GrandTotal:N2}, Due={bill.DueAmount:N2}";
        InvoicePreview =
            $"Invoice #{bill.BillNo} | Subtotal: {bill.Subtotal:N2} | Discount: {bill.DiscountAmount:N2} | " +
            $"Grand: {bill.GrandTotal:N2} | Paid: {bill.PaidAmount:N2} | Due: {bill.DueAmount:N2}";
        Lines.Clear();
        CustomerName = string.Empty;
        PrescriptionNo = string.Empty;
        CustomerMobile = string.Empty;
        CustomerAddress = string.Empty;
        SalesBillNo = $"SB-{DateTime.Now:yyyyMMdd-HHmmss}";
        SelectedDoctorId = null;
        DoctorSearchText = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task QuickAddDoctorAsync()
    {
        var doctorName = string.IsNullOrWhiteSpace(QuickAddDoctorName) ? DoctorSearchText : QuickAddDoctorName;
        if (string.IsNullOrWhiteSpace(doctorName))
        {
            Status = "Doctor name is required.";
            return;
        }

        var normalizedName = doctorName.Trim();
        var existing = await dbContext.Doctors.FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedName.ToLower(), default);
        if (existing is not null)
        {
            SelectedDoctorId = existing.Id;
            DoctorSearchText = existing.Name;
            Status = "Existing doctor selected.";
            ApplyDoctorFilter();
            return;
        }

        var newDoctor = new Doctor
        {
            Name = normalizedName,
            DoctorId = QuickAddDoctorRegistrationNo.Trim(),
            Specialty = QuickAddDoctorSpecialization.Trim(),
            Mobile = QuickAddDoctorMobile.Trim(),
            ClinicName = string.Empty,
            Address = string.Empty,
            Status = "Active",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Doctors.Add(newDoctor);
        await dbContext.SaveChangesAsync(default);

        Doctors.Add(newDoctor);
        Doctors = new ObservableCollection<Doctor>(Doctors.OrderBy(x => x.Name));
        SelectedDoctorId = newDoctor.Id;
        DoctorSearchText = newDoctor.Name;
        QuickAddDoctorName = string.Empty;
        QuickAddDoctorMobile = string.Empty;
        QuickAddDoctorSpecialization = string.Empty;
        QuickAddDoctorRegistrationNo = string.Empty;
        ApplyDoctorFilter();
        Status = "Doctor added and selected.";
    }

    [RelayCommand]
    private async Task CreatePurchaseBillAsync()
    {
        if (!SelectedSupplierId.HasValue)
        {
            Status = "Select supplier.";
            return;
        }

        if (Lines.Count == 0)
        {
            Status = "Add at least one line.";
            return;
        }

        var request = new PurchaseBillCreateRequest
        {
            BillNo = PurchaseBillNo,
            SupplierId = SelectedSupplierId.Value,
            BillDate = BillDate,
            LessDiscount = Math.Max(0, PurchaseLessDiscount),
            RoundOff = PurchaseRoundOff,
            Items = Lines.Select(x => new BillLineDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                BatchNo = string.IsNullOrWhiteSpace(x.BatchNo) ? $"AUTO-{DateTime.UtcNow:yyyyMMdd}-{x.ProductId}" : x.BatchNo,
                StockBatchId = x.StockBatchId,
                ExpiryDate = x.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                Quantity = x.Quantity,
                Mrp = x.Mrp,
                Rate = x.Rate,
                DiscountPercent = x.DiscountPercent,
                GstPercent = x.GstPercent,
                Amount = x.Amount
            }).ToList()
        };

        var bill = await purchaseService.CreateAsync(request, default);
        Status = $"Purchase bill created. Grand={bill.GrandTotal:N2}, GST={bill.CgstAmount + bill.SgstAmount:N2}";
        Lines.Clear();
        PurchaseBillNo = $"PB-{DateTime.Now:yyyyMMdd-HHmmss}";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ExportLastInvoicePdfAsync()
    {
        if (_lastSalesBillId <= 0)
        {
            Status = "Create a sales bill first.";
            return;
        }

        var bytes = await invoiceService.CreateSalesInvoicePdfAsync(_lastSalesBillId, default);
        var dialog = new SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            FileName = $"Invoice-{_lastSalesBillId}.pdf"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await File.WriteAllBytesAsync(dialog.FileName, bytes);
        Status = $"Invoice exported to {dialog.FileName}";
    }

    [RelayCommand]
    private void PreviewCurrentInvoice()
    {
        if (Lines.Count == 0)
        {
            InvoicePreview = "No lines to preview.";
            Status = "Add at least one line to preview GST invoice summary.";
            return;
        }

        InvoicePreview =
            $"Subtotal: {NetAmount:N2} | Line Discount: {LineDiscountAmount:N2} | Taxable: {TaxableAmount:N2} | " +
            $"CGST: {CgstAmount:N2} | SGST: {SgstAmount:N2} | Estimated Grand: {EstimatedGrandTotal:N2}";
        Status = "Invoice preview updated";
    }

    private void FilterBatchesForProduct(int productId)
    {
        if (productId <= 0)
        {
            FilteredBatches = new ObservableCollection<StockBatch>(Batches);
            return;
        }

        FilteredBatches = new ObservableCollection<StockBatch>(
            Batches.Where(x => x.ProductId == productId && x.Quantity > 0).OrderBy(x => x.ExpiryDate));

        if (SelectedBatchId.HasValue && FilteredBatches.All(x => x.Id != SelectedBatchId.Value))
        {
            SelectedBatchId = null;
        }
    }

    private void ApplyProductFilter(string rawKeyword)
    {
        var keyword = rawKeyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            FilteredProducts = new ObservableCollection<Product>(Products.OrderBy(x => x.Name));
            return;
        }

        FilteredProducts = new ObservableCollection<Product>(
            Products
                .Where(x =>
                    x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || Batches.Any(b => b.ProductId == x.Id && b.BatchNo.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(x => x.Name));
    }

    private void ApplyDoctorFilter()
    {
        var keyword = DoctorSearchText.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            FilteredDoctors = new ObservableCollection<Doctor>(Doctors.OrderBy(x => x.Name));
            return;
        }

        FilteredDoctors = new ObservableCollection<Doctor>(
            Doctors
                .Where(x => x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || x.Specialty.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || x.Mobile.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Name));

        if (SelectedDoctorId.HasValue && FilteredDoctors.All(x => x.Id != SelectedDoctorId.Value))
        {
            SelectedDoctorId = null;
        }
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<BackendBillLineRow>())
            {
                item.PropertyChanged += OnLinePropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<BackendBillLineRow>())
            {
                item.PropertyChanged -= OnLinePropertyChanged;
            }
        }

        RecalculateSummaries();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BackendBillLineRow.Quantity)
            or nameof(BackendBillLineRow.Rate)
            or nameof(BackendBillLineRow.DiscountPercent)
            or nameof(BackendBillLineRow.GstPercent))
        {
            RecalculateSummaries();
        }
    }

    private void RecalculateSummaries()
    {
        OnPropertyChanged(nameof(NetAmount));
        OnPropertyChanged(nameof(LineDiscountAmount));
        OnPropertyChanged(nameof(TaxableAmount));
        OnPropertyChanged(nameof(GstAmount));
        OnPropertyChanged(nameof(CgstAmount));
        OnPropertyChanged(nameof(SgstAmount));
        OnPropertyChanged(nameof(EstimatedGrandTotal));
    }
}
