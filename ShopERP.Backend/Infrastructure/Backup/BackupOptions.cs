namespace ShopERP.Backend.Infrastructure.Backup;

public class BackupOptions
{
    public string DatabasePath { get; set; } = "shoperp.db";
    public string BackupDirectory { get; set; } = "Backups";
    public string? AzureBlobConnectionString { get; set; }
    public string? AzureContainerName { get; set; }
}


