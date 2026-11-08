using ResoniteMidiBridge.Config;
using ResoniteMidiBridge.MIDI;
using ResoniteMidiBridge.Networking;
using ResoniteMidiBridge.Playback;
using ResoniteMidiBridge.UI;
using ResoniteMidiBridge.Utilities;

Console.Title = "Resonite MIDI Bridge";

var startupMode = StartupMenu.Show();
if (startupMode == StartupMode.Exit)
{
    Logger.Info("Exiting...");

    return;
}

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

Task.Run(async () =>
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
    switch (startupMode)
    {
        case StartupMode.Live:
            Logger.Info(
                "Starting Live MIDI Mode");

            midiManager.Initialize();

            Logger.Success(
                "Live MIDI system initialized.");

            Logger.Info("");
            Logger.Info("Bridge is running.");
            Logger.Info("Press Q to quit.");
            Logger.Info("");

            bool running = true;

            while (running)
            {
                var key =
                    Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Q:

                        running = false;

                        break;
                }
            }

            break;
    }
}
catch (Exception ex)
{
    Logger.Error(
        $"Startup failed: {ex.Message}");
}

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

Logger.Success(
    "Shutdown complete.");

return;

static async Task RunFileModeAsync(
    WebSocketServer webSocketServer)
{
    var player =
        new MidiFilePlayer(
            webSocketServer);

    var controller =
        new PlaybackController(
            player);

    Console.WriteLine();
    Console.WriteLine(
        "Enter MIDI file path:");

    string? filePath =
        Console.ReadLine();

    if (string.IsNullOrWhiteSpace(filePath))
    {
        Logger.Error(
            "No file path provided.");

        return;
    }

    if (!player.Load(filePath))
    {
        Logger.Error(
            "Failed to load MIDI file.");

        return;
    }

    controller.PrintControls();

    bool running = true;

    while (running)
    {
        var key =
            Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.Q:

                controller.Stop();

                running = false;

                break;

            default:

                await controller.HandleInputAsync(
                    key.Key);

                break;
        }
    }
}