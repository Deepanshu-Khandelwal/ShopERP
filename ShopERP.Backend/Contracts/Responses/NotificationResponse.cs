using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Contracts.Responses;

public class NotificationResponse
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAtUtc { get; set; }
}


