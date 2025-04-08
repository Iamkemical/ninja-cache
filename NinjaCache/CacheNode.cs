using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace NinjaCache;

public class CacheNode
{
    private readonly ConcurrentDictionary<string, string> _store = new();
    private readonly TcpListener _listener;

    public CacheNode(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"Cache node started on port {_listener.LocalEndpoint}");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            var stream = client.GetStream();

            var buffer = new byte[1024];
            int read = await stream.ReadAsync(buffer, 0, buffer.Length);
            var request = Encoding.UTF8.GetString(buffer, 0, read);

            var response = HandleRequest(request);
            var responseBytes = Encoding.UTF8.GetBytes(response);
            
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }

    private string HandleRequest(string request)
    {
        string[] parts;
        string jsonPart = null;
        // Find the first part before the JSON
        if(request.Contains('{'))
        {
            int jsonStartIndex = request.IndexOf('{');
            string commandPart = request.Substring(0, jsonStartIndex).Trim(); // "SET user:1"
            jsonPart = request.Substring(jsonStartIndex);              // The JSON string

            // Put both into an array
            parts = commandPart.Split(" ");
        }
        else
        {
            parts = request.Split(" ");
        }
        
        
        string response;
        switch (parts[0])
        {
            case "SET":
                _store.TryAdd(parts[1], jsonPart);
                response = "OK";
                break;
            case "GET":
                response = _store.TryGetValue(parts[1], out var value) ? value : "NULL";
                break;
            case "DELETE":
                response = _store.TryRemove(parts[1], out _) ? "DELETE" : "NOT FOUND";
                break;
            default: 
                response = "UNKNOWN COMMAND";
                break;
        }
        
        return response;
    }

    public void CleanupStore()
    {
        foreach (var key in _store.Keys)
        {
            _store.TryGetValue(key, out var value);
            var cachedObj = JsonSerializer.Deserialize<CacheEntry<object>>(value);
            if(cachedObj != null)
            {
                if(cachedObj.IsExpired())
                {
                    _store.TryRemove(key, out _);
                }
            }
        }
    }
}