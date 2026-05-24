using ShopERP.Backend.Domain.Common;

namespace ShopERP.Backend.Domain.Entities;

public class BackupLog : EntityBase
{
    public string BackupFileName { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public string CloudPath { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}


