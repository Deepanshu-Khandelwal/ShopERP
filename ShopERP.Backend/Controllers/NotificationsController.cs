using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(INotificationService notificationService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        await notificationService.GenerateSystemNotificationsAsync(ct);
        return Ok(new { message = "Notifications generated." });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool unreadOnly = true, CancellationToken ct = default)
    {
        var query = dbContext.NotificationEntries.AsNoTracking().AsQueryable();
        if (unreadOnly)
        {
            query = query.Where(x => !x.IsRead);
        }

        var data = await query.OrderByDescending(x => x.TriggeredAtUtc).Take(100).ToListAsync(ct);
        return Ok(data);
    }

    [HttpPost("mark-read/{id:int}")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var notification = await dbContext.NotificationEntries.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (notification is null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await dbContext.SaveChangesAsync(ct);
        return Ok(notification);
    }
}


