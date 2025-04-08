using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NinjaCache;

class Program
{
    static async Task Main(string[] args)
    {
        List<Task> tasks = new();

        var node1 = new CacheNode(5000);
        tasks.Add(node1.StartAsync());
        
        var node2 = new CacheNode(5001);
        tasks.Add(node2.StartAsync());
        
        var node3 = new CacheNode(5002);
        tasks.Add(node3.StartAsync());
        
        Thread.Sleep(1000); // wait for server to start
        
        var cache = new DistributedCacheManager(new List<int> { 5000, 5001, 5002 });
        
        Console.WriteLine($"SET Cache => {await cache.Set("user:1", "Gabriel")}");
        Console.WriteLine($"GET Cache => {await cache.Get("user:1")}");
        Console.WriteLine($"DELETE Cache => {await cache.Delete("user:1")}");
        Console.WriteLine($"GET Cache => {await cache.Get("user:1")}");
    }
}