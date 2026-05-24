**ShopERP — Desktop + Backend + JS**

- **Project:** ShopERP (desktop client, .NET backend, JavaScript frontend tooling)
- **Workspace root:** This repository contains the backend (`ShopERP.Backend`), a rebuilt desktop client (`ShopERP.Rebuild`), and a JS workspace (`ShopERP.JS`).

**Overview**
- **Purpose:** Full-featured small business ERP covering sales, purchases, ledgers, suppliers, customers, reports, and sync/backup.
- **Main components:**
  - **Backend:** [ShopERP.Backend](ShopERP.Backend) — ASP.NET Core Web API, EF Core, services and controllers.
  - **Desktop:** [ShopERP.Rebuild/ShopERP.Rebuild.Desktop](ShopERP.Rebuild/ShopERP.Rebuild.Desktop) — WPF (.NET) desktop client.
  - **JS tooling:** [ShopERP.JS](ShopERP.JS) — JS apps and tooling (npm scripts).

**Quick start (developer)**
1. Prerequisites
   - Install .NET SDK (recommended 7.0+ or the version used by the solution).
   - Install Node.js (LTS) for `ShopERP.JS` tasks.
   - SQL Server / SQLite / your chosen DB engine depending on `appsettings`.
2. Run Backend (development)
   - Edit connection string in [ShopERP.Backend/appsettings.Development.json](ShopERP.Backend/appsettings.Development.json) if needed.
   - From repository root:

```
# Run backend (dotnet must be on PATH)
dotnet run --project ShopERP.Backend/ShopERP.Backend.csproj
```

   - Or use the VS Code/VS task: `Run Backend` (see `.vscode/tasks.json` if present).
3. Run Desktop client
```
dotnet run --project ShopERP.Rebuild/ShopERP.Rebuild.Desktop/ShopERP.Rebuild.Desktop.csproj
```
   - Or use `Watch Desktop` task for iterative development.
4. JS apps
```
cd ShopERP.JS
npm install
# run scripts as documented in ShopERP.JS/package.json
```

**Project structure (high level)**
- `ShopERP.Backend/` — API controllers (Controllers/), services (Services/), EF Core context (Data/ShopErpDbContext.cs), configurations, `schema.sql`.
- `ShopERP.Rebuild/` — rebuilt solution with `ShopERP.Rebuild.Desktop` WPF app. Key windows: `LoginWindow.xaml`, `MainWindow.xaml`.
- `ShopERP.JS/` — Node/npm based JS projects for web/desktop assets.
- `logincred.txt` — local credentials file (sensitive). Do not commit credentials to VCS.

**Database & schema**
- SQL schema files: [ShopERP.Backend/schema.sql](ShopERP.Backend/schema.sql) and [ShopERP.Rebuild/schema.sql](ShopERP.Rebuild/schema.sql).
- Use the database engine configured in `appsettings`. Typical steps:
```
# Example: run schema.sql against your DB
# Using sqlcmd / psql / sqlite3 depending on DB
```
- The backend uses EF Core; check `Data/ShopErpDbContext.cs` and `DbInitializer.cs` for seeding logic.

**Configuration & secrets**
- Backend config files: [ShopERP.Backend/appsettings.json](ShopERP.Backend/appsettings.json) and [ShopERP.Backend/appsettings.Development.json](ShopERP.Backend/appsettings.Development.json).
- Do not store secrets in repository. Use environment variables or user secrets for production credentials.
- `logincred.txt` may hold local testing credentials — keep it out of source control.

**Build & run tasks (workspace tasks)**
- Provided tasks (use via VS Code `Run Task`):
  - `Run Backend` — runs `dotnet run --project ShopERP.Backend/ShopERP.Backend.csproj`.
  - `Run Desktop` — runs desktop project.
  - `Watch Desktop` — `dotnet watch` for the desktop app.
  - `Build Solution` — builds `ShopERP.Rebuild.sln`.
  - `Run Backend & Desktop` — helper to start both backend and desktop in sequence.

**Testing**
- There are no explicit test projects in the workspace root. Add unit/integration tests under `tests/` with appropriate test runner (xUnit/NUnit).

**Development notes**
- Services live under `ShopERP.Backend/Services` and expose the core business logic. Controllers wire HTTP endpoints to services.
- Database migrations: either use EF Core migrations (add a `Migrations` project) or apply `schema.sql` manually.
- Search for `TODO` comments across the codebase to find planned improvements.

**Troubleshooting**
- If `dotnet run` fails, ensure the SDK version matches the solution. Run `dotnet --info`.
- Check the backend logs and `appsettings.Development.json` for logging configuration.
- Common error: DB connection refused — validate connection string and ensure DB server running.

**Contributing**
- Fork/branch, open PRs against `main`/`master` branch.
- Keep commits small and focused; include tests for bugfixes/features.
- Update `README.md` when adding new top-level projects.

**Security**
- Never commit secrets or credentials. Use `.gitignore` to keep local credential files untracked.

**License & contact**
- Add a `LICENSE` file if you want an open-source license.
- For questions, open an issue in the repository or contact the maintainers.

---

If you want, I can:
- Add a short `CONTRIBUTING.md` and `LICENSE`.
- Add example `.env` template and a sample local DB setup script.

**Forms**

Below are the main forms used across the desktop and backend admin views, with each form field followed by a one-line description.

- **Login**
   - **Username / Email:** User identifier for authentication.
   - **Password:** Secret used to verify the user's identity.
   - **Remember Me:** Optional flag to persist login on the device.

- **Registration**
   - **Full Name:** The user's display name.
   - **Email:** Contact email and login identifier.
   - **Password:** Account password.
   - **Confirm Password:** Re-enter password to verify.
   - **Company Name:** Optional company or business name.
   - **Phone:** Contact phone number.

- **Product**
   - **Product Code / SKU:** Unique product identifier.
   - **Name:** Product display name.
   - **Description:** Short text describing the product.
   - **Category:** Product category for grouping.
   - **Unit Price:** Selling price per unit.
   - **Cost Price:** Purchase cost per unit.
   - **Tax / GST Rate:** Applicable tax percentage.
   - **Unit of Measure:** e.g., pcs, kg, box.
   - **Opening Stock:** Initial inventory quantity.
   - **Reorder Level:** Threshold to trigger reorder.
   - **Default Supplier:** Primary supplier reference.

- **Customer**
   - **Customer ID:** Internal customer identifier.
   - **Name:** Customer or company name.
   - **Contact Person:** Primary contact at the customer.
   - **Email:** Contact email.
   - **Phone:** Contact phone number.
   - **Billing Address:** Address used for invoicing.
   - **Shipping Address:** Default delivery address.
   - **GSTIN / VAT:** Tax registration number.
   - **Credit Limit:** Max allowed outstanding balance.
   - **Payment Terms:** e.g., Net 30.

- **Supplier**
   - **Supplier ID:** Internal supplier identifier.
   - **Name:** Supplier or company name.
   - **Contact Person:** Primary contact at the supplier.
   - **Email:** Contact email.
   - **Phone:** Contact phone number.
   - **Billing Address:** Supplier billing address.
   - **GSTIN / VAT:** Supplier tax registration number.
   - **Payment Terms:** Supplier payment agreement.

- **Purchase Order (PO)**
   - **PO Number:** Unique PO reference.
   - **Supplier:** Linked supplier for the order.
   - **Order Date:** Date PO was created.
   - **Expected Delivery:** Anticipated arrival date.
   - **Items:** Line items (product, qty, unit price, tax).
   - **Subtotal:** Sum of line item amounts before tax.
   - **Tax Total:** Total tax amount.
   - **Shipping / Freight:** Delivery charges.
   - **Total:** Grand total including tax and shipping.
   - **Status:** e.g., Draft, Ordered, Received, Cancelled.
   - **Notes:** Free-form remarks.

- **Purchase / Supplier Invoice**
   - **Invoice Number:** Supplier invoice reference.
   - **Supplier:** Linked supplier.
   - **Purchase Date:** Date of purchase.
   - **Items:** Purchased line items.
   - **Subtotal / Tax / Total:** Monetary totals.
   - **Payment Method:** e.g., Cash, Bank Transfer.
   - **Reference / Bill No:** Supplier reference number.
   - **Remarks:** Optional notes.

- **Sales Invoice / Sales Order**
   - **Invoice / Order Number:** Unique sales reference.
   - **Customer:** Linked customer.
   - **Date / Due Date:** Invoice and payment due dates.
   - **Items:** Sold line items.
   - **Discount:** Per-line or invoice-level discount.
   - **Tax / GST:** Tax calculations.
   - **Shipping:** Shipping charges.
   - **Total:** Invoice grand total.
   - **Payment Status:** Paid / Unpaid / Partially Paid.

- **Payment**
   - **Payment ID:** Internal payment reference.
   - **Date:** Payment date.
   - **Amount:** Paid amount.
   - **Method:** Payment method used.
   - **Reference:** Bank or transaction reference.
   - **Related Invoice / Purchase:** Linked document.
   - **Notes:** Optional remarks.

- **Ledger Entry / Journal**
   - **Date:** Entry date.
   - **Account:** Ledger account name or code.
   - **Description:** Brief narration.
   - **Debit:** Debit amount.
   - **Credit:** Credit amount.
   - **Reference:** Optional external reference.

- **User Profile / Admin User**
   - **User Name:** Display name.
   - **Email:** Login and contact email.
   - **Role:** User role (Admin, Accountant, Sales).
   - **Permissions:** Granular feature permissions.
   - **Phone:** Contact phone.
   - **Avatar:** Profile image.
   - **Status:** Active / Disabled.

- **Settings / Configuration**
   - **Company Name:** Registered business name.
   - **Address:** Company address.
   - **Financial Year Start:** Fiscal year start date.
   - **Default Currency:** Accounting currency.
   - **Tax Settings:** Default tax/GST configuration.

If you'd like, I can:
- extract form definitions into a machine-readable JSON/YAML spec for the frontend
- add sample XAML snippets for the desktop forms in `ShopERP.Rebuild.Desktop`
- add a `FORMS.md` file with printable reference cards for data entry staff

