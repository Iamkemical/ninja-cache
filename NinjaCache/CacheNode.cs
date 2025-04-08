using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

            var response = await HandleRequest(request);
            var responseBytes = Encoding.UTF8.GetBytes(response);
            
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }

    private async Task<string> HandleRequest(string request)
    {
        var parts = request.Split(' ');
        var command = parts[0].ToUpper();

        string response;
        switch (command)
        {
            case "SET":
                _store.TryAdd(parts[1], parts[2]);
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
}