using ResoniteMidiBridge.Config;
using ResoniteMidiBridge.MIDI;
using ResoniteMidiBridge.Networking;
using ResoniteMidiBridge.Utilities;

Console.Title = "Resonite MIDI Bridge";

Logger.Info("=================================");
Logger.Info("Starting Resonite MIDI Bridge");
Logger.Info("=================================");

var settings = Settings.Instance;

Logger.Info(
    $"WebSocket Endpoint: {settings.GetWebSocketUrl()}");

var webSocketServer =
    new WebSocketServer(
        settings.GetWebSocketUrl());

var midiManager = new MidiManager();

var midiEventHandler =
    new MidiEventHandler(webSocketServer);

midiEventHandler.Subscribe(midiManager);

var webSocketTask = Task.Run(async () =>
{
    try
    {
        await webSocketServer.StartAsync();
    }
    catch (Exception ex)
    {
        Logger.Error(
            $"WebSocket server failed: {ex.Message}");
    }
});

try
{
    midiManager.Initialize();

    Logger.Success(
        "MIDI system initialized successfully.");
}
catch (Exception ex)
{
    Logger.Error(
        $"Failed to initialize MIDI system: {ex.Message}");
}

Logger.Info("");
Logger.Info("Bridge is running.");
Logger.Info("Waiting for MIDI events...");
Logger.Info("Press ENTER to shutdown.");
Logger.Info("");

Console.ReadLine();

Logger.Info("Shutdown requested...");

try
{
    midiManager.Shutdown();
}
catch (Exception ex)
{
    Logger.Error(
        $"Error during MIDI shutdown: {ex.Message}");
}

try
{
    webSocketServer.Stop();
}
catch (Exception ex)
{
    Logger.Error(
        $"Error during WebSocket shutdown: {ex.Message}");
}

Logger.Success("Shutdown complete.");