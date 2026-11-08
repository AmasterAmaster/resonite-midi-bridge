namespace ResoniteMidiBridge.Playback;

public class PlaybackStatus
{
    public PlaybackState State { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int CurrentNoteIndex { get; set; }

    public int TotalNotes { get; set; }

    public TimeSpan CurrentPosition { get; set; }

    public TimeSpan Duration { get; set; }

    public double PercentComplete =>
        Duration.TotalMilliseconds <= 0
            ? 0
            : (CurrentPosition.TotalMilliseconds /
               Duration.TotalMilliseconds) * 100.0;

    public bool IsFinished =>
        State == PlaybackState.Finished;

    public override string ToString()
    {
        return
            $"State={State}, " +
            $"File={FileName}, " +
            $"Position={CurrentPosition:mm\\:ss}/{Duration:mm\\:ss}, " +
            $"Notes={CurrentNoteIndex}/{TotalNotes}, " +
            $"Progress={PercentComplete:F1}%";
    }
}