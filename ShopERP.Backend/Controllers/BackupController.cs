using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/backup")]
public class BackupController(IBackupService backupService, ShopErpDbContext dbContext) : ControllerBase
{
    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        var log = await backupService.RunDailyBackupAsync(ct);
        return Ok(log);
    }

    [HttpPost("restore")]
    public async Task<IActionResult> Restore([FromQuery] string filePath, CancellationToken ct)
    {
        var ok = await backupService.RestoreAsync(filePath, ct);
        return ok ? Ok(new { message = "Restore completed" }) : NotFound(new { message = "Backup file not found" });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(CancellationToken ct)
    {
        return Ok(await dbContext.BackupLogs.AsNoTracking().OrderByDescending(x => x.Id).Take(100).ToListAsync(ct));
    }
}


