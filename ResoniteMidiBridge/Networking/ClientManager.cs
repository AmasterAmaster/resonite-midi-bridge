using System.Net.WebSockets;
using System.Text;

namespace ResoniteMidiBridge.Networking;

public class ClientManager
{
    private readonly List<WebSocket> _clients = new();

    public IReadOnlyCollection<WebSocket> Clients => _clients;

    public void AddClient(WebSocket client)
    {
        _clients.Add(client);

        Console.WriteLine(
            $"Client connected. Total clients: {_clients.Count}");
    }

    public void RemoveClient(WebSocket client)
    {
        _clients.Remove(client);

        Console.WriteLine(
            $"Client disconnected. Total clients: {_clients.Count}");
    }

    public async Task BroadcastAsync(string message)
    {
        if (_clients.Count == 0)
            return;

        byte[] buffer = Encoding.UTF8.GetBytes(message);

        var disconnectedClients = new List<WebSocket>();

        foreach (var client in _clients)
        {
            try
            {
                if (client.State != WebSocketState.Open)
                {
                    disconnectedClients.Add(client);
                    continue;
                }

                await client.SendAsync(
                    buffer,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch
            {
                disconnectedClients.Add(client);
            }
        }

        foreach (var client in disconnectedClients)
        {
            RemoveClient(client);
        }
    }
}