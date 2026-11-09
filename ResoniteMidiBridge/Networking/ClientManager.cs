using System.Net.WebSockets;
using System.Text;
using ResoniteMidiBridge.Utilities;

namespace ResoniteMidiBridge.Networking;

public class ClientManager
{
    private readonly List<WebSocket> _clients = new();

    public IReadOnlyCollection<WebSocket> Clients => _clients;

    private readonly object _clientLock = new();

    private readonly SemaphoreSlim _sendLock =
    new(1, 1);

    public void AddClient(WebSocket client)
    {
        lock (_clientLock)
        {
            _clients.Add(client);
        }

        Logger.Info(
            $"Client connected. Total clients: {_clients.Count}");
    }

    public void RemoveClient(WebSocket client)
    {
        lock (_clientLock)
        {
            _clients.Remove(client);
        }

        Logger.Info(
            $"Client disconnected. Total clients: {_clients.Count}");
    }

    public async Task BroadcastAsync(string message)
    {
        if (_clients.Count == 0)
            return;

        byte[] buffer = Encoding.UTF8.GetBytes(message);

        var disconnectedClients = new List<WebSocket>();

        List<WebSocket> clients;

        lock (_clientLock)
        {
            clients = _clients.ToList();
        }

        foreach (var client in clients)
        {
            await _sendLock.WaitAsync();

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
            finally
            {
                _sendLock.Release();
            }
        }

        foreach (var client in disconnectedClients)
        {
            RemoveClient(client);
        }
    }
}