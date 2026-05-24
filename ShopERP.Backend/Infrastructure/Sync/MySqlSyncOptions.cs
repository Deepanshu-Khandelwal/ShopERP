namespace ShopERP.Backend.Infrastructure.Sync;

public sealed class MySqlSyncOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string CheckpointName { get; set; } = "default";
}


