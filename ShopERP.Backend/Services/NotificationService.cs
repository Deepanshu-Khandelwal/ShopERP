using Microsoft.EntityFrameworkCore;
using ShopERP.Backend.Data;
using ShopERP.Backend.Domain.Entities;
using ShopERP.Backend.Domain.Enums;

namespace ShopERP.Backend.Services;

public class NotificationService(ShopErpDbContext dbContext) : INotificationService
{
    public async Task GenerateSystemNotificationsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow.Date;
        var ninetyDaysFromNow = now.AddDays(90);

        var recentSales = await dbContext.StockMovements
            .AsNoTracking()
            .Where(x => x.MovementType == StockMovementType.Sale && x.MovementDate >= now.AddDays(-30) && x.Quantity < 0)
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                SoldUnits = -g.Sum(x => x.Quantity)
            })
            .ToListAsync(ct);

        var salesRateByProduct = recentSales.ToDictionary(x => x.ProductId, x => Math.Round(x.SoldUnits / 30m, 2));

        var stockBatches = await dbContext.StockBatches
            .AsNoTracking()
            .Select(x => new
            {
                x.ProductId,
                ProductName = x.Product.Name,
                x.BatchNo,
                x.Quantity,
                x.ExpiryDate,
                LowStockThreshold = x.Product.LowStockThreshold
            })
            .ToListAsync(ct);

        foreach (var low in stockBatches.Where(x => x.Quantity <= x.LowStockThreshold))
        {
            dbContext.NotificationEntries.Add(new NotificationEntry
            {
                Type = NotificationType.LowStock,
                Title = "Low stock alert",
                Message = $"{low.ProductName} ({low.BatchNo}) has only {low.Quantity} units left."
            });
        }

        foreach (var batch in stockBatches.Where(x => x.Quantity > 0))
        {
            if (!salesRateByProduct.TryGetValue(batch.ProductId, out var avgDailySales) || avgDailySales <= 0)
            {
                continue;
            }

            var daysLeft = Math.Round(batch.Quantity / avgDailySales, 1);
            if (daysLeft <= 15)
            {
                dbContext.NotificationEntries.Add(new NotificationEntry
                {
                    Type = NotificationType.LowStock,
                    Title = "Low stock prediction",
                    Message = $"{batch.ProductName} ({batch.BatchNo}) may run out in about {daysLeft} days at current sales rate."
                });
            }
        }

        foreach (var item in stockBatches.Where(x => x.ExpiryDate >= now && x.ExpiryDate <= ninetyDaysFromNow))
        {
            var days = (item.ExpiryDate - now).Days;
            string? band = days switch
            {
                <= 30 => "30-day",
                <= 60 => "60-day",
                <= 90 => "90-day",
                _ => null
            };

            if (band is not null)
            {
                dbContext.NotificationEntries.Add(new NotificationEntry
                {
                    Type = NotificationType.NearExpiry,
                    Title = "Expiry alert",
                    Message = $"{item.ProductName} batch {item.BatchNo} is in {band} expiry window ({days} days left)."
                });
            }
        }

        var dueCustomers = await dbContext.CustomerLedgerEntries
            .AsNoTracking()
            .GroupBy(x => new { x.CustomerId, x.Customer.Name })
            .Select(g => new
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.Name,
                Balance = g.OrderByDescending(x => x.Id).Select(x => x.Balance).FirstOrDefault()
            })
            .Where(x => x.Balance > 0)
            .ToListAsync(ct);

        foreach (var due in dueCustomers)
        {
            dbContext.NotificationEntries.Add(new NotificationEntry
            {
                Type = NotificationType.PaymentReminder,
                Title = "Payment due reminder",
                Message = $"Customer {due.CustomerName ?? due.CustomerId.ToString()} has pending dues of {due.Balance:0.00}."
            });
        }

        await dbContext.SaveChangesAsync(ct);
    }
}


