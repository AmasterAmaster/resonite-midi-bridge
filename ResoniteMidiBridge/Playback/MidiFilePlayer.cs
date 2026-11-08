using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using ResoniteMidiBridge.Messages;
using ResoniteMidiBridge.Networking;
using ResoniteMidiBridge.Utilities;

namespace ResoniteMidiBridge.Playback;

public class MidiFilePlayer
{
    private readonly object _playbackLock = new();

    private List<Note> _notes = new();

    private TempoMap? _tempoMap;

    private int _currentNoteIndex = 0;

    private long _currentMidiTime = 0;

    private Task? _playbackTask;

    private readonly WebSocketServer _webSocketServer;

    private MidiFile? _midiFile;

    private PlaybackState _state = PlaybackState.Stopped;

    private CancellationTokenSource? _playbackToken;

    private string _loadedFile = string.Empty;

    public PlaybackState State => _state;

    public string LoadedFile => _loadedFile;

    public bool IsLoaded => _midiFile != null;

    public int CurrentNoteIndex
    => _currentNoteIndex;

    public int TotalNotes
        => _notes.Count;

    private TimeSpan _currentPosition =
    TimeSpan.Zero;

    private TimeSpan _duration =
    TimeSpan.Zero;

    public MidiFilePlayer(WebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
    }

    public bool Load(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Logger.Error(
                    $"File does not exist: {filePath}");

                return false;
            }

            _midiFile = MidiFile.Read(filePath);
            _tempoMap = _midiFile.GetTempoMap();

            _notes = _midiFile
                .GetNotes()
                .OrderBy(n => n.Time)
                .ToList();

            if (_notes.Count > 0)
            {
                var lastNote = _notes.Last();

                var endTime =
                    lastNote.Time + lastNote.Length;

                var durationMetric =
                    TimeConverter.ConvertTo<MetricTimeSpan>(
                        endTime,
                        _tempoMap);

                _duration =
                    TimeSpan.FromMilliseconds(
                        durationMetric.TotalMicroseconds / 1000.0);
            }
            else
            {
                _duration = TimeSpan.Zero;
            }

            _currentNoteIndex = 0;
            _currentMidiTime = 0;

            _loadedFile = filePath;

            _state = PlaybackState.Stopped;

            Logger.Success(
                $"Loaded MIDI file: {Path.GetFileName(filePath)}");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(
                $"Failed to load MIDI file: {ex.Message}");

            _state = PlaybackState.Error;

            return false;
        }
    }

    public async Task PlayAsync()
    {
        if (!IsLoaded)
        {
            Logger.Warning(
                "No MIDI file loaded.");

            return;
        }

        if (_state == PlaybackState.Playing)
        {
            return;
        }

        _playbackToken =
            new CancellationTokenSource();

        _state = PlaybackState.Playing;

        Logger.Info(
            $"Playing: {Path.GetFileName(_loadedFile)}");

        _playbackTask =
            Task.Run(() =>
                PlaybackLoopAsync(
                    _playbackToken.Token));

        await Task.CompletedTask;
    }

    public void Pause()
    {
        if (_state != PlaybackState.Playing)
        {
            return;
        }

        _playbackToken?.Cancel();

        _state = PlaybackState.Paused;

        Logger.Info(
            "Playback paused.");
    }

    public void Stop()
    {
        _playbackToken?.Cancel();

        _currentNoteIndex = 0;
        _currentMidiTime = 0;
        _currentPosition = TimeSpan.Zero;

        _state = PlaybackState.Stopped;

        Logger.Info(
            "Playback stopped.");
    }

    public void SeekToNote(int noteIndex)
    {
        if (!_notes.Any())
        {
            return;
        }

        noteIndex = Math.Clamp(
            noteIndex,
            0,
            _notes.Count - 1);

        _currentNoteIndex = noteIndex;

        var note = _notes[noteIndex];

        _currentMidiTime = note.Time;

        var metricPosition =
            TimeConverter.ConvertTo<MetricTimeSpan>(
                note.Time,
                _tempoMap!);

        _currentPosition =
            TimeSpan.FromMilliseconds(
                metricPosition.TotalMicroseconds / 1000.0);

        Logger.Info(
            $"Seeked to note {noteIndex}");
    }

    public void SeekByPercentage(
    double percentage)
    {
        if (!_notes.Any())
        {
            return;
        }

        percentage =
            Math.Clamp(
                percentage,
                0,
                100);

        int targetIndex =
            (int)((percentage / 100.0) *
                (_notes.Count - 1));

        SeekToNote(targetIndex);
    }

    public void Seek(
    TimeSpan position)
    {
        if (!_notes.Any())
        {
            return;
        }

        position =
            TimeSpan.FromMilliseconds(
                Math.Clamp(
                    position.TotalMilliseconds,
                    0,
                    _duration.TotalMilliseconds));

        var targetNote =
            _notes.FindIndex(n =>
            {
                var metric =
                    TimeConverter.ConvertTo<MetricTimeSpan>(
                        n.Time,
                        _tempoMap!);

                var noteTime =
                    TimeSpan.FromMilliseconds(
                        metric.TotalMicroseconds / 1000.0);

                return noteTime >= position;
            });

        if (targetNote < 0)
        {
            targetNote =
                _notes.Count - 1;
        }

        SeekToNote(targetNote);
    }

    private async Task PlaybackLoopAsync(
    CancellationToken token)
    {
        while (
            _currentNoteIndex < _notes.Count &&
            !token.IsCancellationRequested)
        {
            var note =
                _notes[_currentNoteIndex];

            long deltaTime =
                note.Time - _currentMidiTime;

            var metricTime =
                TimeConverter.ConvertTo<MetricTimeSpan>(
                    deltaTime,
                    _tempoMap!);

            int delayMs =
                (int)(metricTime.TotalMicroseconds / 1000);

            if (delayMs > 0)
            {
                await Task.Delay(
                    delayMs,
                    token);
            }

            await SendNoteOn(note);

            var noteLength =
                TimeConverter.ConvertTo<MetricTimeSpan>(
                    note.Length,
                    _tempoMap!);

            int noteLengthMs =
                (int)(noteLength.TotalMicroseconds / 1000);

            _ = SendDelayedNoteOff(
                note,
                noteLengthMs);

            _currentMidiTime = note.Time;
            var currentMetric =
            TimeConverter.ConvertTo<MetricTimeSpan>(
                _currentMidiTime,
                _tempoMap!);

            _currentPosition =
                TimeSpan.FromMilliseconds(
                    currentMetric.TotalMicroseconds / 1000.0);

            _currentNoteIndex++;
        }

        if (_currentNoteIndex >= _notes.Count)
        {
            _state = PlaybackState.Finished;

            Logger.Success(
                "MIDI playback completed.");
        }
    }

    private async Task SendNoteOn(Note note)
    {
        var message = new MidiMessage
        {
            Type = MessageTypes.NoteOn,
            Channel = (int)note.Channel,
            Note = note.NoteNumber,
            Velocity = note.Velocity,
            Timestamp =
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceDevice = "MidiFile"
        };

        await BroadcastMessage(message);
    }

    private async Task SendNoteOff(Note note)
    {
        var message = new MidiMessage
        {
            Type = MessageTypes.NoteOff,
            Channel = (int)note.Channel,
            Note = note.NoteNumber,
            Velocity = 0,
            Timestamp =
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceDevice = "MidiFile"
        };

        await BroadcastMessage(message);
    }

    private async Task SendDelayedNoteOff(
        Note note,
        int delayMs)
    {
        try
        {
            await Task.Delay(delayMs);

            await SendNoteOff(note);
        }
        catch
        {
            // Ignore
        }
    }

    private async Task BroadcastMessage(
        MidiMessage message)
    {
        string json = message.ToJson();

        Logger.Debug(
            $"FILE => {json}");

        await _webSocketServer
            .BroadcastAsync(json);
    }

    public PlaybackStatus GetStatus()
    {
        return new PlaybackStatus
        {
            State = _state,
            FileName = Path.GetFileName(_loadedFile),
            CurrentNoteIndex = _currentNoteIndex,
            TotalNotes = _notes.Count,
            CurrentPosition = _currentPosition,
            Duration = _duration
        };
    }
}