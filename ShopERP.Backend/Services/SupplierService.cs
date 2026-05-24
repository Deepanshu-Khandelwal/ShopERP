using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Services;

public class SupplierService(ShopErpDbContext dbContext) : ISupplierService
{
    public async Task<Supplier> GetSupplierOrThrowAsync(int supplierId, CancellationToken ct)
    {
        return await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == supplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found.");
    }
}


