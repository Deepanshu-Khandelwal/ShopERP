-- ShopERP.Backend SQLite schema
-- Generated from entity models + DbContext configuration

PRAGMA foreign_keys = ON;

BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS Suppliers (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Address TEXT NOT NULL,
    Email TEXT NOT NULL,
    DrugLicenseNo TEXT NOT NULL,
    GstNo TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Customers (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Phone TEXT NOT NULL,
    Email TEXT NOT NULL,
    Address TEXT NOT NULL,
    DateOfBirth TEXT NOT NULL,
    Gender TEXT NOT NULL,
    BloodGroup TEXT NOT NULL,
    MedicalHistory TEXT NOT NULL,
    EmergencyContact TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Doctors (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    DoctorId TEXT NOT NULL,
    Specialty TEXT NOT NULL,
    Degree TEXT NOT NULL,
    Mobile TEXT NOT NULL,
    ClinicName TEXT NOT NULL,
    Address TEXT NOT NULL,
    Email TEXT NOT NULL,
    ClinicVisitDetails TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Active',
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    GenericName TEXT NOT NULL,
    Manufacturer TEXT NOT NULL,
    Category TEXT NOT NULL,
    Sku TEXT NOT NULL,
    HsnCode TEXT NOT NULL,
    GstPercent REAL NOT NULL DEFAULT 0,
    Mrp REAL NOT NULL,
    PurchaseRate REAL NOT NULL,
    SaleRate REAL NOT NULL,
    LowStockThreshold INTEGER NOT NULL DEFAULT 10,
    IsPrescriptionRequired INTEGER NOT NULL DEFAULT 0,
    Status TEXT NOT NULL DEFAULT 'Active',
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS StockBatches (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ProductId INTEGER NOT NULL,
    BatchNo TEXT NOT NULL,
    ExpiryDate TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    PurchaseRate REAL NOT NULL,
    SaleRate REAL NOT NULL,
    Mrp REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_StockBatches_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS StockMovements (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ProductId INTEGER NOT NULL,
    StockBatchId INTEGER NULL,
    MovementType INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    ReferenceNo TEXT NOT NULL,
    MovementDate TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_StockMovements_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockMovements_StockBatches_StockBatchId FOREIGN KEY (StockBatchId) REFERENCES StockBatches(Id) ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS PurchaseBills (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    BillNo TEXT NOT NULL,
    BillDate TEXT NOT NULL,
    SupplierId INTEGER NOT NULL,
    Subtotal REAL NOT NULL,
    DiscountAmount REAL NOT NULL,
    CgstAmount REAL NOT NULL,
    SgstAmount REAL NOT NULL,
    RoundOff REAL NOT NULL,
    GrandTotal REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseBills_Suppliers_SupplierId FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PurchaseItems (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    PurchaseBillId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    BatchNo TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    Mrp REAL NOT NULL,
    Rate REAL NOT NULL,
    ExpiryDate TEXT NOT NULL,
    DiscountPercent REAL NOT NULL,
    GstPercent REAL NOT NULL,
    Amount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseItems_PurchaseBills_PurchaseBillId FOREIGN KEY (PurchaseBillId) REFERENCES PurchaseBills(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS SalesBills (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    BillNo TEXT NOT NULL,
    BillDate TEXT NOT NULL,
    PaymentType INTEGER NOT NULL,
    CustomerId INTEGER NULL,
    DoctorId INTEGER NULL,
    Subtotal REAL NOT NULL,
    DiscountPercent REAL NOT NULL,
    DiscountAmount REAL NOT NULL,
    CgstAmount REAL NOT NULL,
    SgstAmount REAL NOT NULL,
    TotalGstAmount REAL NOT NULL,
    GrandTotal REAL NOT NULL,
    ProfitAmount REAL NOT NULL,
    PaidAmount REAL NOT NULL,
    DueAmount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_SalesBills_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_SalesBills_Doctors_DoctorId FOREIGN KEY (DoctorId) REFERENCES Doctors(Id) ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS SalesItems (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    SalesBillId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    StockBatchId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    Mrp REAL NOT NULL,
    SaleRate REAL NOT NULL,
    PurchaseRate REAL NOT NULL,
    DiscountPercent REAL NOT NULL,
    GstPercent REAL NOT NULL,
    TaxableAmount REAL NOT NULL,
    GstAmount REAL NOT NULL,
    CgstAmount REAL NOT NULL,
    SgstAmount REAL NOT NULL,
    Amount REAL NOT NULL,
    ProfitAmount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_SalesItems_SalesBills_SalesBillId FOREIGN KEY (SalesBillId) REFERENCES SalesBills(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SalesItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SalesItems_StockBatches_StockBatchId FOREIGN KEY (StockBatchId) REFERENCES StockBatches(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PurchaseReturns (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ReturnNo TEXT NOT NULL,
    SupplierId INTEGER NOT NULL,
    ReturnDate TEXT NOT NULL,
    TotalAmount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseReturns_Suppliers_SupplierId FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PurchaseReturnItems (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    PurchaseReturnId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    StockBatchId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    Rate REAL NOT NULL,
    Amount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseReturnItems_PurchaseReturns_PurchaseReturnId FOREIGN KEY (PurchaseReturnId) REFERENCES PurchaseReturns(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS SalesReturns (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ReturnNo TEXT NOT NULL,
    CustomerId INTEGER NULL,
    ReturnDate TEXT NOT NULL,
    RefundAmount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_SalesReturns_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS SalesReturnItems (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    SalesReturnId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    StockBatchId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    SaleRate REAL NOT NULL,
    Amount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_SalesReturnItems_SalesReturns_SalesReturnId FOREIGN KEY (SalesReturnId) REFERENCES SalesReturns(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PaymentEntries (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    PartyType INTEGER NOT NULL,
    PartyId INTEGER NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate TEXT NOT NULL,
    ReferenceNo TEXT NOT NULL,
    Notes TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS SupplierLedgerEntries (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    SupplierId INTEGER NOT NULL,
    EntryDate TEXT NOT NULL,
    VoucherNo TEXT NOT NULL,
    PurchaseAmount REAL NOT NULL,
    PaymentAmount REAL NOT NULL,
    Balance REAL NOT NULL,
    Narration TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_SupplierLedgerEntries_Suppliers_SupplierId FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS CustomerLedgerEntries (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    CustomerId INTEGER NOT NULL,
    EntryDate TEXT NOT NULL,
    VoucherNo TEXT NOT NULL,
    BillAmount REAL NOT NULL,
    PaymentAmount REAL NOT NULL,
    Balance REAL NOT NULL,
    Narration TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_CustomerLedgerEntries_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PurchaseOrders (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    OrderNo TEXT NOT NULL,
    SupplierId INTEGER NOT NULL,
    OrderDate TEXT NOT NULL,
    Status INTEGER NOT NULL,
    TotalAmount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseOrders_Suppliers_SupplierId FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PurchaseOrderItems (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    PurchaseOrderId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    Rate REAL NOT NULL,
    Amount REAL NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseOrderItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS NotificationEntries (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Type INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Message TEXT NOT NULL,
    IsRead INTEGER NOT NULL DEFAULT 0,
    TriggeredAtUtc TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS BackupLogs (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    BackupFileName TEXT NOT NULL,
    LocalPath TEXT NOT NULL,
    CloudPath TEXT NOT NULL,
    IsSuccess INTEGER NOT NULL DEFAULT 0,
    Message TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Shops (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Address TEXT NOT NULL,
    City TEXT NOT NULL,
    State TEXT NOT NULL,
    ZipCode TEXT NOT NULL,
    Phone TEXT NOT NULL,
    Email TEXT NOT NULL,
    GstNo TEXT NOT NULL,
    RegistrationNo TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ShopId INTEGER NOT NULL,
    Username TEXT NOT NULL,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    PhoneNumber TEXT NOT NULL,
    Role INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    LastLoginUtc TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_Users_Shops_ShopId FOREIGN KEY (ShopId) REFERENCES Shops(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS UserProfiles (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    Address TEXT NULL,
    City TEXT NULL,
    State TEXT NULL,
    ZipCode TEXT NULL,
    ProfilePictureUrl TEXT NULL,
    DepartmentOrdesignation TEXT NULL,
    DateOfBirth TEXT NULL,
    PreferredLanguage TEXT NULL DEFAULT 'en',
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    CONSTRAINT FK_UserProfiles_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Indexes from OnModelCreating + performance indexes
CREATE INDEX IF NOT EXISTS IX_Suppliers_Name ON Suppliers(Name);
CREATE INDEX IF NOT EXISTS IX_Suppliers_GstNo ON Suppliers(GstNo);
CREATE INDEX IF NOT EXISTS IX_Products_Name ON Products(Name);
CREATE UNIQUE INDEX IF NOT EXISTS IX_StockBatches_ProductId_BatchNo ON StockBatches(ProductId, BatchNo);
CREATE INDEX IF NOT EXISTS IX_StockBatches_ExpiryDate ON StockBatches(ExpiryDate);
CREATE INDEX IF NOT EXISTS IX_StockBatches_ProductId ON StockBatches(ProductId);
CREATE INDEX IF NOT EXISTS IX_StockMovements_MovementType_MovementDate_ProductId ON StockMovements(MovementType, MovementDate, ProductId);

CREATE UNIQUE INDEX IF NOT EXISTS IX_PurchaseBills_BillNo ON PurchaseBills(BillNo);
CREATE INDEX IF NOT EXISTS IX_PurchaseBills_BillDate ON PurchaseBills(BillDate);
CREATE UNIQUE INDEX IF NOT EXISTS IX_SalesBills_BillNo ON SalesBills(BillNo);
CREATE INDEX IF NOT EXISTS IX_SalesBills_BillDate ON SalesBills(BillDate);
CREATE UNIQUE INDEX IF NOT EXISTS IX_PurchaseReturns_ReturnNo ON PurchaseReturns(ReturnNo);
CREATE INDEX IF NOT EXISTS IX_PurchaseReturns_ReturnDate ON PurchaseReturns(ReturnDate);
CREATE UNIQUE INDEX IF NOT EXISTS IX_SalesReturns_ReturnNo ON SalesReturns(ReturnNo);
CREATE INDEX IF NOT EXISTS IX_SalesReturns_ReturnDate ON SalesReturns(ReturnDate);
CREATE UNIQUE INDEX IF NOT EXISTS IX_PurchaseOrders_OrderNo ON PurchaseOrders(OrderNo);
CREATE INDEX IF NOT EXISTS IX_PurchaseOrders_OrderDate ON PurchaseOrders(OrderDate);
CREATE INDEX IF NOT EXISTS IX_PaymentEntries_PaymentDate ON PaymentEntries(PaymentDate);
CREATE INDEX IF NOT EXISTS IX_NotificationEntries_IsRead_TriggeredAtUtc ON NotificationEntries(IsRead, TriggeredAtUtc);
CREATE INDEX IF NOT EXISTS IX_BackupLogs_CreatedAtUtc ON BackupLogs(CreatedAtUtc);
CREATE INDEX IF NOT EXISTS IX_CustomerLedgerEntries_CustomerId ON CustomerLedgerEntries(CustomerId);
CREATE INDEX IF NOT EXISTS IX_CustomerLedgerEntries_EntryDate ON CustomerLedgerEntries(EntryDate);
CREATE INDEX IF NOT EXISTS IX_SupplierLedgerEntries_SupplierId ON SupplierLedgerEntries(SupplierId);
CREATE INDEX IF NOT EXISTS IX_SupplierLedgerEntries_EntryDate ON SupplierLedgerEntries(EntryDate);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Shops_Name ON Shops(Name);
CREATE INDEX IF NOT EXISTS IX_Shops_Email ON Shops(Email);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);
CREATE INDEX IF NOT EXISTS IX_Users_ShopId ON Users(ShopId);
CREATE UNIQUE INDEX IF NOT EXISTS IX_UserProfiles_UserId ON UserProfiles(UserId);

COMMIT;
