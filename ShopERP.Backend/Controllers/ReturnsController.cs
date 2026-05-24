using Microsoft.AspNetCore.Mvc;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/returns")]
public class ReturnsController(IReturnService returnService) : ControllerBase
{
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseReturn([FromBody] ReturnCreateRequest request, CancellationToken ct)
    {
        var result = await returnService.CreatePurchaseReturnAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("sales")]
    public async Task<IActionResult> SalesReturn([FromBody] ReturnCreateRequest request, CancellationToken ct)
    {
        var result = await returnService.CreateSalesReturnAsync(request, ct);
        return Ok(result);
    }
}


