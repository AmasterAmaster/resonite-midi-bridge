using ResoniteMidiBridge.Utilities;

namespace ResoniteMidiBridge.Playback;

public class PlaybackController
{
    private readonly MidiFilePlayer _player;

    public PlaybackController(MidiFilePlayer player)
    {
        _player = player;
    }

    public PlaybackState State => _player.State;

    public bool IsLoaded => _player.IsLoaded;

    public enum PlaybackFinishedAction
    {
        Replay,
        LoadNewFile,
        MainMenu,
        Quit
    }

    public async Task PlayAsync()
    {
        if (!IsLoaded)
        {
            Logger.Warning(
                "Cannot play. No MIDI file loaded.");

            return;
        }

        Logger.Info("Play requested.");

        await _player.PlayAsync();
    }

    public void Pause()
    {
        if (!IsLoaded)
        {
            Logger.Warning(
                "Cannot pause. No MIDI file loaded.");

            return;
        }

        Logger.Info("Pause requested.");

        _player.Pause();
    }

    public void Stop()
    {
        if (!IsLoaded)
        {
            Logger.Warning(
                "Cannot stop. No MIDI file loaded.");

            return;
        }

        Logger.Info("Stop requested.");

        _player.Stop();
    }

    public async Task TogglePlayPauseAsync()
    {
        switch (State)
        {
            case PlaybackState.Playing:
                Pause();
                break;

            case PlaybackState.Paused:
            case PlaybackState.Stopped:
            case PlaybackState.Finished:
                await PlayAsync();
                break;

            default:
                Logger.Warning(
                    $"Cannot toggle playback from state: {State}");
                break;
        }
    }

    public void PrintStatus()
    {
        Console.WriteLine();
        Console.WriteLine("=================================");
        Console.WriteLine(" Playback Status");
        Console.WriteLine("=================================");
        Console.WriteLine($"State: {State}");
        Console.WriteLine($"Loaded: {IsLoaded}");
        Console.WriteLine();
    }

    public async Task HandleInputAsync(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.P:

                if (State == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    await PlayAsync();
                }

                break;

            case ConsoleKey.S:

                Stop();

                break;

            case ConsoleKey.I:

                PrintStatus();

                break;
        }
    }

    public void PrintControls()
    {
        Console.WriteLine();
        Console.WriteLine("=================================");
        Console.WriteLine(" MIDI File Controls");
        Console.WriteLine("=================================");
        Console.WriteLine("[P] Play / Pause");
        Console.WriteLine("[S] Stop");
        Console.WriteLine("[I] Info");
        Console.WriteLine("[Q] Quit");
        Console.WriteLine();
    }

    public PlaybackFinishedAction ShowFinishedMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=================================");
        Console.WriteLine(" Playback Complete");
        Console.WriteLine("=================================");
        Console.WriteLine();
        Console.WriteLine("[P] Play Again");
        Console.WriteLine("[L] Load New MIDI File");
        Console.WriteLine("[M] Main Menu");
        Console.WriteLine("[Q] Quit");
        Console.WriteLine();

        while (true)
        {
            var key =
                Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.P:
                    return PlaybackFinishedAction.Replay;

                case ConsoleKey.L:
                    return PlaybackFinishedAction.LoadNewFile;

                case ConsoleKey.M:
                    return PlaybackFinishedAction.MainMenu;

                case ConsoleKey.Q:
                    return PlaybackFinishedAction.Quit;
            }
        }
    }
}