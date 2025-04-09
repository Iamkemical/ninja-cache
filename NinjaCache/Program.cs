using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NinjaCache;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<CleanupStoreBackgroundService>();
            })
        .Build();

        List<Task> tasks = [];

        var node1 = new CacheNode(5000);
        tasks.Add(node1.StartAsync());
        
        var node2 = new CacheNode(5001);
        tasks.Add(node2.StartAsync());
        
        var node3 = new CacheNode(5002);
        tasks.Add(node3.StartAsync());
        
        Thread.Sleep(1000); // wait for server to start

        //Fire the host(background service) on a separate task
        tasks.Add(host.RunAsync());
        
        var cache = new DistributedCacheManager(new List<int> { 5000, 5001, 5002 });

        while(true)
        {
            Console.WriteLine("\nSelect the following options\n1. Create new person\n2. Retrieve person\n3. Delete person\n");
            var option = int.Parse(Console.ReadLine());

            switch(option)
            {
                case 1:
                    await Set(cache);
                    break;
                case 2:
                    await Get(cache);
                    break;
                case 3:
                    await Delete(cache);
                    break;
                default:
                    Console.WriteLine($"Invalid option. Try again");
                    break;
            }
        }
    }

    public static async Task Get(DistributedCacheManager cache)
    {
        Console.Write($"Name: ");
        var name = Console.ReadLine();
        var res = await cache.Get($"{name}:1");
        CacheEntry<Person> cacheEntry = null;
        if(res != "NULL")
        {
            cacheEntry = JsonSerializer.Deserialize<CacheEntry<Person>>(res);
        }

        var cacheRsp = cacheEntry != null ? $"\nName: {cacheEntry.Value.Name}\nAge: {cacheEntry.Value.Age}\nAddress: {cacheEntry.Value.Address}" : "NULL";
        Console.WriteLine($"\nGET Cache => {cacheRsp}");
    }

    public static async Task Delete(DistributedCacheManager cache)
    {
        Console.Write($"Name: ");

        var name = Console.ReadLine();
        Console.WriteLine($"\nDELETE Cache => {await cache.Delete($"{name}:1")}");
    }

    public static async Task Set(DistributedCacheManager cache)
    {
        Console.WriteLine($"\nEnter new Person\n");
        Console.Write($"Name: ");
        var name = Console.ReadLine();

        Console.Write($"Age: ");
        var age = int.Parse(Console.ReadLine());

        Console.Write($"Address: ");
        var address = Console.ReadLine();

        Console.WriteLine($"\nSET Cache => {await cache.Set($"{name}:1", new Person
        {
            Name = name,
            Age = age,
            Address = address
        }, DateTimeOffset.UtcNow.AddSeconds(25))}");
    }
}