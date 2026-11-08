namespace ResoniteMidiBridge.Messages;

public enum MessageTypes
{
    NoteOn,
    NoteOff,
    ControlChange,
    PitchBend,

    TransportPlay,
    TransportStop,
    TransportPause,

    TempoChanged,

    Unknown,

    PlaybackStarted,
    PlaybackPaused,
    PlaybackStopped,
    PlaybackFinished,

    PlaybackPosition,

    DeviceConnected,
    DeviceDisconnected
}