-- ShopERP.Rebuild local SQLite schema
-- Generated from entity models + DbContext configuration

PRAGMA foreign_keys = ON;

BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS Products (
    Id TEXT NOT NULL PRIMARY KEY,
    Sku TEXT NOT NULL,
    Name TEXT NOT NULL,
    GenericName TEXT NOT NULL,
    Manufacturer TEXT NOT NULL,
    Category TEXT NOT NULL,
    BatchNo TEXT NOT NULL,
    Expiry TEXT NULL,
    Price REAL NOT NULL,
    SaleRate REAL NOT NULL,
    Mrp REAL NOT NULL,
    GstPercent REAL NOT NULL,
    StockQty INTEGER NOT NULL,
    MinStockLevel INTEGER NOT NULL DEFAULT 10,
    Status TEXT NOT NULL DEFAULT 'Active',
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Customers (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL,
    Phone TEXT NOT NULL,
    Email TEXT NOT NULL,
    Address TEXT NOT NULL,
    MedicalHistory TEXT NOT NULL,
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Sales (
    Id TEXT NOT NULL PRIMARY KEY,
    InvoiceNo TEXT NOT NULL,
    CustomerId TEXT NULL,
    Total REAL NOT NULL,
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL,
    CONSTRAINT FK_Sales_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS SaleLines (
    Id TEXT NOT NULL PRIMARY KEY,
    SaleId TEXT NOT NULL,
    ProductId TEXT NOT NULL,
    ProductName TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice REAL NOT NULL,
    TaxAmount REAL NOT NULL,
    DiscountAmount REAL NOT NULL,
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL,
    CONSTRAINT FK_SaleLines_Sales_SaleId FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SaleLines_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Purchases (
    Id TEXT NOT NULL PRIMARY KEY,
    BillNo TEXT NOT NULL,
    SupplierName TEXT NOT NULL,
    Total REAL NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Open',
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS PurchaseLines (
    Id TEXT NOT NULL PRIMARY KEY,
    PurchaseId TEXT NOT NULL,
    ItemName TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice REAL NOT NULL,
    BatchNo TEXT NOT NULL,
    ExpiryDate TEXT NULL,
    Mrp REAL NOT NULL,
    TaxAmount REAL NOT NULL,
    DiscountAmount REAL NOT NULL,
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL,
    CONSTRAINT FK_PurchaseLines_Purchases_PurchaseId FOREIGN KEY (PurchaseId) REFERENCES Purchases(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS SyncStates (
    Id INTEGER NOT NULL PRIMARY KEY,
    LastSyncedUtc TEXT NOT NULL,
    LastError TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS UserAccounts (
    Id TEXT NOT NULL PRIMARY KEY,
    Username TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,
    Role INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedUtc TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Products_Sku ON Products(Sku);
CREATE INDEX IF NOT EXISTS IX_Products_BatchNo ON Products(BatchNo);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Sales_InvoiceNo ON Sales(InvoiceNo);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Purchases_BillNo ON Purchases(BillNo);
CREATE UNIQUE INDEX IF NOT EXISTS IX_UserAccounts_Username ON UserAccounts(Username);

INSERT OR IGNORE INTO SyncStates (Id, LastSyncedUtc, LastError)
VALUES (1, '0001-01-01T00:00:00', '');

COMMIT;
