using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController(ShopErpDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
    {
        var query = dbContext.Suppliers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x => x.Name.Contains(q) || x.GstNo.Contains(q) || x.DrugLicenseNo.Contains(q));
        }

        return Ok(await query.OrderBy(x => x.Name).ToListAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Supplier supplier, CancellationToken ct)
    {
        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(ct);
        return Ok(supplier);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Supplier input, CancellationToken ct)
    {
        var supplier = await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (supplier is null)
        {
            return NotFound();
        }

        supplier.Name = input.Name;
        supplier.Address = input.Address;
        supplier.Email = input.Email;
        supplier.DrugLicenseNo = input.DrugLicenseNo;
        supplier.GstNo = input.GstNo;
        supplier.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(supplier);
    }
}


