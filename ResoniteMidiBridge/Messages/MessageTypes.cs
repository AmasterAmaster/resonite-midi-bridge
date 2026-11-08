namespace ResoniteMidiBridge.Messages;

public enum MessageTypes
{
    NoteOn,
    NoteOff,
    ControlChange,
    PitchBend,

    DeviceConnected,
    DeviceDisconnected,

    TransportPlay,
    TransportStop,
    TransportPause,

    TempoChanged,

    Unknown
}