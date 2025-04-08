using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaCache;

public class CleanupStoreBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Background service starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            //Console.WriteLine("Running background task at: {time}", DateTimeOffset.Now);
            List<int> ports = [5000, 5001, 5002];
            foreach (var port in ports)
            {
                CacheNode node = new(port);
                node.CleanupStore();
            }
            await Task.Delay(10000, stoppingToken); // Simulate work
        }

        Console.WriteLine("Background service stopping.");
    }
}