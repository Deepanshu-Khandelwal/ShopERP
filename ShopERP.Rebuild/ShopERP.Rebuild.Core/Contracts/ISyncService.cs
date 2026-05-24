using ShopERP.Rebuild.Core.Domain.Enums;

namespace ShopERP.Rebuild.Core.Contracts;

public interface ISyncService
{
    SyncHealth CurrentHealth { get; }
    string LastMessage { get; }
    DateTime LastRunUtc { get; }
    Task SyncAsync(CancellationToken cancellationToken = default);
}
