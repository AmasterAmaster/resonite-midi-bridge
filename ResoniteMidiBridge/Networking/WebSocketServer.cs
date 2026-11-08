using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace ResoniteMidiBridge.Networking;

public class WebSocketServer
{
    private readonly HttpListener _listener;
    private readonly ClientManager _clientManager;

    private bool _isRunning;

    public ClientManager ClientManager => _clientManager;

    public WebSocketServer(string url)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);

        _clientManager = new ClientManager();
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"WebSocket server started.");
        Console.WriteLine($"Listening on {_listener.Prefixes.First()}");

        while (_isRunning)
        {
            HttpListenerContext context =
                await _listener.GetContextAsync();

            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                continue;
            }

            _ = HandleClientAsync(context);
        }
    }

    private async Task HandleClientAsync(HttpListenerContext context)
    {
        WebSocketContext wsContext =
            await context.AcceptWebSocketAsync(null);

        WebSocket socket = wsContext.WebSocket;

        _clientManager.AddClient(socket);

        Console.WriteLine("WebSocket client accepted.");

        await ReceiveLoopAsync(socket);

        _clientManager.RemoveClient(socket);

        try
        {
            await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Disconnected",
                CancellationToken.None);
        }
        catch
        {
            // Ignore
        }
    }

    private async Task ReceiveLoopAsync(WebSocket socket)
    {
        byte[] buffer = new byte[4096];

        while (socket.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result =
                    await socket.ReceiveAsync(
                        buffer,
                        CancellationToken.None);

                if (result.MessageType ==
                    WebSocketMessageType.Close)
                {
                    break;
                }

                string message = Encoding.UTF8.GetString(
                    buffer,
                    0,
                    result.Count);

                Console.WriteLine(
                    $"Received: {message}");

                await HandleIncomingMessage(message);
            }
            catch
            {
                break;
            }
        }
    }

    private Task HandleIncomingMessage(string message)
    {
        Console.WriteLine($"Message Handler: {message}");

        /*
         * Future:
         *
         * Json -> MidiMessage
         *
         * Control Resonite
         * Control MIDI Devices
         * Transport Events
         * Configuration
         */

        return Task.CompletedTask;
    }

    public async Task BroadcastAsync(string message)
    {
        await _clientManager.broadcastAsync(message);
    }

    public void Stop()
    {
        _wsRunning = false;

        _listener.Stop();

        Console.WriteLine("WebSocket server stopped.");
    }
}