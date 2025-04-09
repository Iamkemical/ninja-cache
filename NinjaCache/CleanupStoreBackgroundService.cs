using Microsoft.Extensions.Hosting;

namespace NinjaCache;

public class CleanupStoreBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Background service strating...");

        while(!stoppingToken.IsCancellationRequested)
        {
            List<int> ports = [5000, 5001, 5002];
            foreach(int port in ports)
            {
                CacheNode node = new(port);
                node.CleanupStore();
            }

            await Task.Delay(15000, stoppingToken); 
        }

        Console.WriteLine("Background service stopping");
    }
}
