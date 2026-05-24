using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Infrastructure.Backup;

public class BackupService(ShopErpDbContext dbContext, IOptions<BackupOptions> options, IWebHostEnvironment environment) : IBackupService
{
    private readonly BackupOptions _options = options.Value;

    public async Task<BackupLog> RunDailyBackupAsync(CancellationToken ct)
    {
        var backupDir = Path.Combine(environment.ContentRootPath, _options.BackupDirectory);
        Directory.CreateDirectory(backupDir);

        var sourceDb = Path.Combine(environment.ContentRootPath, _options.DatabasePath);
        var fileName = $"shoperp_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db";
        var destination = Path.Combine(backupDir, fileName);

        var log = new BackupLog
        {
            BackupFileName = fileName,
            LocalPath = destination,
            IsSuccess = true,
            Message = "Backup completed"
        };

        try
        {
            await CopyFileAsync(sourceDb, destination, ct);
            await CopySidecarFileIfPresentAsync(sourceDb + "-wal", destination + "-wal", ct);
            await CopySidecarFileIfPresentAsync(sourceDb + "-shm", destination + "-shm", ct);

            if (!string.IsNullOrWhiteSpace(_options.AzureBlobConnectionString) && !string.IsNullOrWhiteSpace(_options.AzureContainerName))
            {
                var blobService = new BlobServiceClient(_options.AzureBlobConnectionString);
                var container = blobService.GetBlobContainerClient(_options.AzureContainerName);
                await container.CreateIfNotExistsAsync(cancellationToken: ct);

                await using var stream = File.OpenRead(destination);
                var blob = container.GetBlobClient(fileName);
                await blob.UploadAsync(stream, overwrite: true, cancellationToken: ct);
                log.CloudPath = blob.Uri.ToString();
            }
        }
        catch (Exception ex)
        {
            log.IsSuccess = false;
            log.Message = ex.Message;
        }

        dbContext.BackupLogs.Add(log);
        await dbContext.SaveChangesAsync(ct);
        return log;
    }

    public async Task<bool> RestoreAsync(string backupFilePath, CancellationToken ct)
    {
        if (!File.Exists(backupFilePath))
        {
            return false;
        }

        var sourceDb = Path.Combine(environment.ContentRootPath, _options.DatabasePath);
        await CopyFileAsync(backupFilePath, sourceDb, ct);
        await CopySidecarFileIfPresentAsync(backupFilePath + "-wal", sourceDb + "-wal", ct);
        await CopySidecarFileIfPresentAsync(backupFilePath + "-shm", sourceDb + "-shm", ct);
        return true;
    }

    private static Task CopySidecarFileIfPresentAsync(string sourcePath, string destinationPath, CancellationToken ct)
    {
        if (!File.Exists(sourcePath))
        {
            return Task.CompletedTask;
        }

        return CopyFileAsync(sourcePath, destinationPath, ct);
    }

    private static async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken ct)
    {
        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        await using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await sourceStream.CopyToAsync(destinationStream, ct);
    }
}


