using Microsoft.AspNetCore.Mvc;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController(IMySqlSyncService mySqlSyncService) : ControllerBase
{
    [HttpPost("mysql")]
    public async Task<IActionResult> SyncToMySql([FromQuery] bool fullSync = false, CancellationToken ct = default)
    {
        var result = await mySqlSyncService.SyncToMySqlAsync(fullSync, ct);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}


