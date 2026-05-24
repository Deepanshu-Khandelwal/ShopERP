using ShopERP.Backend.Domain.Common;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Domain.Entities;

public class NotificationEntry : EntityBase
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;
}


