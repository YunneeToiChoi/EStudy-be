using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using study4_be.Services.Backup;

public class BackupSchedulerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;

    public BackupSchedulerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Set up a timer that runs every 10 seconds
        _timer = new Timer(DoBackup, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        Console.WriteLine("Backup service started. Scheduled to run every 10 seconds.");
        return Task.CompletedTask;
    }

    private void DoBackup(object state)
    {
        // Create a new scope to resolve scoped services
        using (var scope = _serviceProvider.CreateScope())
        {
            var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();

            Console.WriteLine($"Starting backup at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}...");

            try
            {
                backupService.BackupDataAsync().Wait(); // Call your backup method here
                Console.WriteLine("Backup completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during backup: {ex.Message}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        Console.WriteLine("Backup service stopped.");
        return Task.CompletedTask;
    }
}