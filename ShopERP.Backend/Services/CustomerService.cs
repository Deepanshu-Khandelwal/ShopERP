using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;

namespace ShopERP.Backend.Services;

public class CustomerService(ShopErpDbContext dbContext) : ICustomerService
{
    public async Task<int?> ResolveOrCreateCustomerIdAsync(int? customerId, string? customerName, string? customerMobile, string? customerAddress, CancellationToken ct)
    {
        if (customerId.HasValue)
        {
            return customerId;
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return null;
        }

        var customer = new Customer
        {
            Name = customerName.Trim(),
            Phone = customerMobile?.Trim() ?? string.Empty,
            Address = customerAddress?.Trim() ?? string.Empty
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(ct);
        return customer.Id;
    }

    public async Task<int?> ResolveOrCreateDoctorIdAsync(int? doctorId, string? doctorName, CancellationToken ct)
    {
        if (doctorId.HasValue)
        {
            return doctorId;
        }

        if (string.IsNullOrWhiteSpace(doctorName))
        {
            return null;
        }

        var normalized = doctorName.Trim();
        var existing = await dbContext.Doctors
            .FirstOrDefaultAsync(x => x.Name.ToLower() == normalized.ToLower(), ct);

        if (existing is not null)
        {
            return existing.Id;
        }

        var doctor = new Doctor { Name = normalized };
        dbContext.Doctors.Add(doctor);
        await dbContext.SaveChangesAsync(ct);
        return doctor.Id;
    }
}


