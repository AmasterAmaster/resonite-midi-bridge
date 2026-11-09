using ResoniteMidiBridge.Config;
using ResoniteMidiBridge.MIDI;
using ResoniteMidiBridge.Networking;
using ResoniteMidiBridge.Playback;
using ResoniteMidiBridge.UI;
using ResoniteMidiBridge.Utilities;

Console.Title = "Resonite MIDI Bridge";

Logger.Info("=================================");
Logger.Info("Starting Resonite MIDI Bridge");
Logger.Info("=================================");

var settings = Settings.Instance;

var webSocketServer =
    new WebSocketServer(
        settings.GetWebSocketUrl());

_ = Task.Run(async () =>
{
    try
    {
        await webSocketServer.StartAsync();
    }
    catch (Exception ex)
    {
        Logger.Error(
            $"WebSocket server failed: {ex}");
    }
});

bool applicationRunning = true;

while (applicationRunning)
{
    var startupMode =
        StartupMenu.Show();

    switch (startupMode)
    {
        case StartupMode.Exit:

            applicationRunning = false;

            break;

        case StartupMode.Live:

            await RunLiveModeAsync(
                webSocketServer);

            break;

        case StartupMode.File:

            await RunFileModeAsync(
                webSocketServer);

            break;
    }
}

Logger.Info("Shutdown requested...");

try
{
    webSocketServer.Stop();
}
catch (Exception ex)
{
    Logger.Error(
        $"WebSocket shutdown error: {ex}");
}

Logger.Success("Shutdown complete.");

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
        if (player.IsFinished)
        {
            var action =
                controller.ShowFinishedMenu();

            switch (action)
            {
                case PlaybackController
                        .PlaybackFinishedAction.Replay:

                    player.Stop();

                    await player.PlayAsync();

                    break;

                case PlaybackController
                        .PlaybackFinishedAction.LoadNewFile:

                    return;

                case PlaybackController
                        .PlaybackFinishedAction.MainMenu:

                    return;

                case PlaybackController
                        .PlaybackFinishedAction.Quit:

                    Environment.Exit(0);

                    break;
            }
        }

        var key =
            Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.Q:

                Environment.Exit(0);

                break;
            case ConsoleKey.M:

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

static async Task RunLiveModeAsync(
    WebSocketServer webSocketServer)
{
    var midiManager =
        new MidiManager();

    var midiHandler =
        new MidiEventHandler(
            webSocketServer);

    midiHandler.Subscribe(
        midiManager);

    midiManager.Initialize();

    Logger.Success(
        "Live MIDI mode started.");

    Logger.Info(
        "Press M for Main Menu.");

    bool running = true;

    while (running)
    {
        var key =
            Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.M:

                running = false;

                break;
        }
    }

    midiManager.Shutdown();
}