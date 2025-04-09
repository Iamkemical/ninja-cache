using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace NinjaCache;

public class DistributedCacheManager
{
    private readonly List<int> _ports;

    public DistributedCacheManager(List<int> ports)
    {
        _ports = ports;
    }

    private int GetNodePort(string key)
    {
        // Simple hash-based routing
        int hash = Math.Abs(key.GetHashCode());
        return _ports[hash % _ports.Count];
    }

    private async Task<string> SendCommand(int port, string command)
    {
        using var client = new TcpClient("localhost", port);
        var stream = client.GetStream();
        var data = Encoding.UTF8.GetBytes(command);
        await stream.WriteAsync(data, 0, data.Length);
        
        var buffer = new byte[1024];
        int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, bytes);
    }

    public async Task<string> Set(string key, object value, DateTimeOffset expiryTime)
    {
        int port = GetNodePort(key);
        CacheEntry<object> cacheEntry = new()
        {
            Value = value,
            ExpiryTime = expiryTime
        };
        var jsonString = JsonSerializer.Serialize(cacheEntry);
        return await SendCommand(port, $"SET {key} {jsonString}");
    }

    public async Task<string> Get(string key)
    {
        int port = GetNodePort(key);
        var res = await SendCommand(port, $"GET {key}");
        if(res != "NULL")
        {
            var rsp = JsonSerializer.Deserialize<CacheEntry<object>>(res);
            res = rsp.IsExpired() ? "NULL" : res;
        }

        return res;
    }

    public async Task<string> Delete(string key)
    {
        int port = GetNodePort(key);
        return await SendCommand(port, $"DELETE {key}");
    }
}