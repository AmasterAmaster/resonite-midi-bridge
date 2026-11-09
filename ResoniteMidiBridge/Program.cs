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

            bool liveRunning = true;

            while (liveRunning)
            {
                var key =
                    Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Q:

                        liveRunning = false;

                        break;
                }
            }

            break;

        case StartupMode.File:

            Logger.Info(
                "Starting MIDI File Mode");

            await RunFileModeAsync(
                webSocketServer);

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
    filePath = filePath?.Trim();

    if (!string.IsNullOrWhiteSpace(filePath))
    {
        filePath =
            filePath.Trim('"');
    }

    if (string.IsNullOrWhiteSpace(filePath))
    {
        Logger.Error(
            "No file path provided.");

        return;
    }

    if (!File.Exists(filePath))
    {
        Logger.Error(
            "File does not exist.");

        return;
    }

    var extension =
        Path.GetExtension(filePath);

    if (!extension.Equals(
            ".mid",
            StringComparison.OrdinalIgnoreCase) &&
        !extension.Equals(
            ".midi",
            StringComparison.OrdinalIgnoreCase))
    {
        Logger.Error(
            "File is not a MIDI file.");

        return;
    }

    if (!player.Load(filePath))
    {
        Logger.Error(
            "Failed to load MIDI file.");

        return;
    }

    var status =
    player.GetStatus();

    Logger.Success(
        $"Loaded {status.FileName}");

    Logger.Info(
        $"Duration: {status.Duration:mm\\:ss}");

    Logger.Info(
        $"Total Notes: {status.TotalNotes}");

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