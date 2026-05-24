namespace ShopERP.Rebuild.Core.Configuration;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public int IntervalSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = true;
    public string AesKey { get; set; } = "replace-with-32-char-key-1234567890";
}
