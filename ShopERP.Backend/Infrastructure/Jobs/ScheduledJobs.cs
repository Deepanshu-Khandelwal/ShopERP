using Hangfire;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Infrastructure.Jobs;

public static class ScheduledJobs
{
    public static void Configure()
    {
        RecurringJob.AddOrUpdate<IBackupService>("daily-backup", x => x.RunDailyBackupAsync(CancellationToken.None), Cron.Daily);
        RecurringJob.AddOrUpdate<INotificationService>("daily-alerts", x => x.GenerateSystemNotificationsAsync(CancellationToken.None), Cron.Daily);
    }
}


