namespace ShopERP.Rebuild.Core.Domain.Entities;

public sealed class SyncState
{
    public int Id { get; set; }
    public DateTime LastSyncedUtc { get; set; }
    public string LastError { get; set; } = string.Empty;
}
