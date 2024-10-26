using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using study4_be.Services.Backup;

public class BackupSchedulerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer; // Timer cho sao lưu 5 giây, nhưng không được sử dụng
    private Timer _weeklyTimer;

    public BackupSchedulerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Set up a timer that runs every 10 seconds, but we won't use it
        _timer = new Timer(DoBackup, null, Timeout.Infinite, Timeout.Infinite);
        // nếu chạy thì chỉ cần ghi 
        // _timer = new Timer(DoBackup, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        Console.WriteLine("Backup service started. Scheduled to run every 10 seconds (not used).");

        // Set up a timer for weekly backup
        var nextMonday = GetNextMonday();
        var delay = nextMonday - DateTime.UtcNow;

        _weeklyTimer = new Timer(DoWeeklyBackup, null, delay, TimeSpan.FromDays(7));
        Console.WriteLine("Weekly backup service scheduled.");

        return Task.CompletedTask;
    }


    private async void DoBackup(object state)
    {
        // Create a new scope to resolve scoped services
        using (var scope = _serviceProvider.CreateScope())
        {
            var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();
            Console.WriteLine($"Starting backup at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}...");

            try
            {
                await backupService.BackupDataAsync(); // Await the backup method
                Console.WriteLine("Backup completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during backup: {ex.Message}");
                // Optionally log more details about the exception
            }
        }
    }

    private async void DoWeeklyBackup(object state)
    {
        // Create a new scope to resolve scoped services
        using (var scope = _serviceProvider.CreateScope())
        {
            var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();
            Console.WriteLine($"Starting weekly backup at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}...");

            try
            {
                await backupService.BackupDataAsync(); // Await the backup method
                Console.WriteLine("Weekly backup completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during weekly backup: {ex.Message}");
                // Optionally log more details about the exception
            }
        }
    }

    private DateTime GetNextMonday()
    {
        var today = DateTime.UtcNow;
        var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilNextMonday == 0) daysUntilNextMonday = 7; // Set to next week if today is Monday
        return today.Date.AddDays(daysUntilNextMonday).AddHours(0); // Replace 0 with desired hour if needed
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0); // Dừng timer 5 giây
        _weeklyTimer?.Change(Timeout.Infinite, 0); // Dừng timer hàng tuần
        Console.WriteLine("Backup service stopped.");
        return Task.CompletedTask;
    }
}
