using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Desktop.Services;
using ShopERP.Rebuild.Desktop.ViewModels;
using ShopERP.Rebuild.Desktop.ViewModels.Pages;
using ShopERP.Rebuild.Infrastructure.Data;
using ShopERP.Rebuild.Infrastructure.Extensions;
using System.IO;
using System.Windows;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;
using RebuildDbContext = ShopERP.Rebuild.Infrastructure.Data.ShopErpDbContext;
using ShopERP.Backend.Infrastructure.Pdf;
using ShopERP.Backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Data;
using System.Windows.Threading;

namespace ShopERP.Rebuild.Desktop;

public partial class App : Application
{
	private IHost? _host;
	private IServiceScope? _appScope;
	private string _dataRoot = string.Empty;

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		DispatcherUnhandledException += OnDispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

		_dataRoot = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"ShopERP.Rebuild");
		Directory.CreateDirectory(_dataRoot);

		var localSqlitePath = Path.Combine(_dataRoot, "shoperp_rebuild_local.db");
		var backendSqlitePath = Path.Combine(_dataRoot, "shoperp_rebuild_backend.db");

		_host = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration(config =>
			{
				config.SetBasePath(AppContext.BaseDirectory);
				config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
				config.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["ConnectionStrings:LocalSqlite"] = $"Data Source={localSqlitePath}",
					["ConnectionStrings:ShopERP"] = $"Data Source={backendSqlitePath}"
				});
			})
			.ConfigureServices((context, services) =>
			{
				services.AddDbContext<BackendDbContext>(options => options.UseSqlite(context.Configuration.GetConnectionString("ShopERP")!));
				services.AddRebuildInfrastructure(context.Configuration);
				services.AddScoped<IGstService, GstService>();
				services.AddScoped<IStockService, StockService>();
				services.AddScoped<ISalesCalculationService, SalesCalculationService>();
				services.AddScoped<ICustomerService, CustomerService>();
				services.AddScoped<ILedgerService, LedgerService>();
				services.AddScoped<IPurchaseCalculationService, PurchaseCalculationService>();
				services.AddScoped<ISupplierService, SupplierService>();
				services.AddScoped<ISupplierLedgerService, SupplierLedgerService>();
				services.AddScoped<IPurchaseService, PurchaseService>();
				services.AddScoped<ISalesService, SalesService>();
				services.AddScoped<IReturnService, ReturnService>();
				services.AddScoped<IPaymentService, PaymentService>();
				services.AddScoped<IReportService, ReportService>();
				services.AddScoped<INotificationService, NotificationService>();
				services.AddScoped<IInvoiceService, InvoiceService>();
				services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
				services.AddScoped<UserSession>();
				services.AddScoped<ProductCrudService>();
				services.AddScoped<ProductMasterService>();
				services.AddScoped<CustomerCrudService>();
				services.AddScoped<DoctorCrudService>();
				services.AddScoped<SalesCrudService>();
				services.AddScoped<PurchasesCrudService>();
				services.AddSingleton<IShellNavigationService, ShellNavigationService>();
				services.AddSingleton<PaymentDraftStore>();
				services.AddSingleton<PurchaseOrderDraftStore>();

				services.AddScoped<DashboardPageViewModel>();
				services.AddScoped<ProductsPageViewModel>();
				services.AddScoped<ProductsFormPageViewModel>();
				services.AddScoped<CustomersPageViewModel>();
				services.AddScoped<CustomersFormPageViewModel>();
				services.AddScoped<DoctorsPageViewModel>();
				services.AddScoped<DoctorsFormPageViewModel>();
				services.AddScoped<SalesPageViewModel>();
				services.AddScoped<SalesFormPageViewModel>();
				services.AddScoped<PurchasesPageViewModel>();
				services.AddScoped<PurchasesFormPageViewModel>();
				services.AddScoped<ReportsPageViewModel>();
				services.AddScoped<StockPageViewModel>();
				services.AddScoped<NotificationsPageViewModel>();
				services.AddScoped<LedgersPageViewModel>();
				services.AddScoped<BillingPageViewModel>();
				services.AddScoped<BillingFormPageViewModel>();
				services.AddScoped<ReturnsPageViewModel>();
				services.AddScoped<ReturnsFormPageViewModel>();
				services.AddScoped<PaymentsPageViewModel>();
				services.AddScoped<PaymentsFormPageViewModel>();
				services.AddScoped<PurchaseOrdersPageViewModel>();
				services.AddScoped<PurchaseOrdersFormPageViewModel>();
				services.AddScoped<SyncPageViewModel>();

				services.AddScoped<LoginViewModel>();
				services.AddScoped<RegistrationViewModel>();
				services.AddScoped<LoginWindow>();
				services.AddScoped<RegistrationWindow>();
				services.AddScoped<MainViewModel>();
				services.AddScoped<MainWindow>();
			})
			.Build();

		await _host.StartAsync();

		_appScope = _host.Services.CreateScope();
		var scopedProvider = _appScope.ServiceProvider;

		using (var scope = _host.Services.CreateScope())
		{
			var localDb = scope.ServiceProvider.GetRequiredService<RebuildDbContext>();
			bool localNeedsReset = false;
			try { await localDb.Database.ExecuteSqlRawAsync("SELECT BatchNo FROM Products LIMIT 1;"); }
			catch { localNeedsReset = true; }

			if (localNeedsReset) await localDb.Database.EnsureDeletedAsync();
			await localDb.Database.EnsureCreatedAsync();

			var db = scope.ServiceProvider.GetRequiredService<BackendDbContext>();
			await EnsureBackendSchemaReadyAsync(db);
			BackendDbContext.EnsurePerformanceIndexes(db);
		}

		var authService = scopedProvider.GetRequiredService<ShopERP.Rebuild.Core.Contracts.IAuthenticationService>();
		await authService.SeedDefaultAdminAsync();

		var loginWindow = scopedProvider.GetRequiredService<LoginWindow>();
		var loginOk = loginWindow.ShowDialog();
		if (loginOk != true)
		{
			Shutdown();
			return;
		}

		var window = scopedProvider.GetRequiredService<MainWindow>();
		MainWindow = window;
		ShutdownMode = ShutdownMode.OnMainWindowClose;
		window.Show();
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		DispatcherUnhandledException -= OnDispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
		TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

		if (_host is not null)
		{
			_appScope?.Dispose();
			await _host.StopAsync();
			_host.Dispose();
		}

		base.OnExit(e);
	}

	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		var logPath = WriteCrashLog("DispatcherUnhandledException", e.Exception);
		MessageBox.Show(
			$"Unexpected error occurred. Log saved at:\n{logPath}",
			"ShopERP Error",
			MessageBoxButton.OK,
			MessageBoxImage.Error);
		e.Handled = true;
	}

	private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			WriteCrashLog("AppDomainUnhandledException", ex);
		}
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		WriteCrashLog("UnobservedTaskException", e.Exception);
		e.SetObserved();
	}

	private string WriteCrashLog(string source, Exception ex)
	{
		var root = string.IsNullOrWhiteSpace(_dataRoot)
			? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShopERP.Rebuild")
			: _dataRoot;

		var logDir = Path.Combine(root, "logs");
		Directory.CreateDirectory(logDir);
		var logPath = Path.Combine(logDir, "app-errors.log");

		var payload = new StringBuilder();
		payload.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}");
		payload.AppendLine(ex.ToString());
		payload.AppendLine(new string('-', 100));

		// Write to local appdata logs
		File.AppendAllText(logPath, payload.ToString());

		// Also mirror logs to workspace Documents/ShopERP/logs for easier access
		try
		{
			var workspaceLogs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShopERP", "logs");
			Directory.CreateDirectory(workspaceLogs);
			var workspaceLogPath = Path.Combine(workspaceLogs, "app-errors.log");
			File.AppendAllText(workspaceLogPath, payload.ToString());
			return workspaceLogPath;
		}
		catch
		{
			return logPath;
		}
	}

	private static async Task EnsureBackendSchemaReadyAsync(BackendDbContext db)
	{
		await db.Database.EnsureCreatedAsync();

		var requiredTables = new[]
		{
			"Products",
			"StockBatches",
			"SalesBills",
			"PurchaseBills",
			"Customers",
			"Suppliers",
			"Doctors",
			"PaymentEntries",
			"CustomerLedgerEntries",
			"SupplierLedgerEntries"
		};

		var missing = await GetMissingTablesAsync(db, requiredTables);
		
		// Check for new medical columns to trigger reset if needed
		bool needsReset = missing.Count > 0;
		if (!needsReset)
		{
			// Try to find one of the new columns, if it fails, we need reset
			try { await db.Database.ExecuteSqlRawAsync("SELECT GenericName FROM Products LIMIT 1;"); }
			catch { needsReset = true; }
		}

		if (!needsReset)
		{
			return;
		}

		await db.Database.EnsureDeletedAsync();
		await db.Database.EnsureCreatedAsync();
	}

	public Task LogoutAsync(Window currentWindow)
	{
		if (_host is null)
		{
			return Task.CompletedTask;
		}

		var previousScope = _appScope;
		var loginScope = _host.Services.CreateScope();
		currentWindow.Hide();

		try
		{
			var loginWindow = loginScope.ServiceProvider.GetRequiredService<LoginWindow>();
			loginWindow.Owner = currentWindow;
			var loginOk = loginWindow.ShowDialog();
			if (loginOk != true)
			{
				loginScope.Dispose();
				currentWindow.Show();
				return Task.CompletedTask;
			}

			var nextMainWindow = loginScope.ServiceProvider.GetRequiredService<MainWindow>();
			MainWindow = nextMainWindow;
			ShutdownMode = ShutdownMode.OnMainWindowClose;
			nextMainWindow.Show();

			previousScope?.Dispose();
			_appScope = loginScope;
			currentWindow.Close();
			return Task.CompletedTask;
		}
		catch
		{
			loginScope.Dispose();
			currentWindow.Show();
			throw;
		}
	}

	private static async Task<List<string>> GetMissingTablesAsync(BackendDbContext db, IEnumerable<string> requiredTables)
	{
		var missing = new List<string>();
		var connection = db.Database.GetDbConnection();

		if (connection.State != ConnectionState.Open)
		{
			await connection.OpenAsync();
		}

		foreach (var tableName in requiredTables)
		{
			await using var command = connection.CreateCommand();
			command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name LIMIT 1;";
			var parameter = command.CreateParameter();
			parameter.ParameterName = "$name";
			parameter.Value = tableName;
			command.Parameters.Add(parameter);

			var result = await command.ExecuteScalarAsync();
			if (result is null)
			{
				missing.Add(tableName);
			}
		}

		await connection.CloseAsync();
		return missing;
	}
}

