using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Domain.Entities;
using BackendDbContext = ShopERP.Backend.Data.ShopErpDbContext;

namespace ShopERP.Rebuild.Desktop.Services;

public sealed class DoctorCrudService(BackendDbContext dbContext)
{
    public Task<List<Doctor>> ListAsync(CancellationToken cancellationToken = default)
        => dbContext.Doctors
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Doctor model, CancellationToken cancellationToken = default)
    {
        model.UpdatedAtUtc = DateTime.UtcNow;

        var exists = await dbContext.Doctors.AnyAsync(x => x.Id == model.Id, cancellationToken);
        if (!exists)
        {
            model.CreatedAtUtc = DateTime.UtcNow;
            dbContext.Doctors.Add(model);
        }
        else
        {
            dbContext.Doctors.Update(model);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Doctor model, CancellationToken cancellationToken = default)
    {
        dbContext.Doctors.Remove(model);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}