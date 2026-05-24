using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopERP.Rebuild.Core.Configuration;
using ShopERP.Rebuild.Core.Contracts;
using ShopERP.Rebuild.Infrastructure.Auth;
using ShopERP.Rebuild.Infrastructure.Data;
using ShopERP.Rebuild.Infrastructure.Repositories;
using ShopERP.Rebuild.Infrastructure.Services;

namespace ShopERP.Rebuild.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRebuildInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var localConnection = configuration.GetConnectionString("LocalSqlite")
            ?? "Data Source=shoperp_rebuild.db";

        services.AddDbContext<ShopErpDbContext>(options => options.UseSqlite(localConnection));

        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ISyncService, DataSyncService>();
        services.AddHostedService<SyncBackgroundWorker>();

        return services;
    }
}
