using Melanchall.DryWetMidi.Core;
using ResoniteMidiBridge.Messages;

namespace ResoniteMidiBridge.MIDI;

public static class MidiMessageBuilder
{
    public static MidiMessage Build(NoteOnEvent e)
    {
        return new MidiMessage
        {
            Type = MessageTypes.NoteOn,
            Channel = (int)e.Channel,
            Note = e.NoteNumber,
            Velocity = e.Velocity,
            Timestamp = GetTimestamp()
        };
    }

    public static MidiMessage Build(NoteOffEvent e)
    {
        return new MidiMessage
        {
            Type = MessageTypes.NoteOff,
            Channel = (int)e.Channel,
            Note = e.NoteNumber,
            Velocity = e.Velocity,
            Timestamp = GetTimestamp()
        };
    }

    public static MidiMessage Build(ControlChangeEvent e)
    {
        return new MidiMessage
        {
            Type = MessageTypes.ControlChange,
            Channel = (int)e.Channel,
            Controller = (int)e.ControlNumber,
            Value = e.ControlValue,
            Timestamp = GetTimestamp()
        };
    }

    public static MidiMessage Build(PitchBendEvent e)
    {
        return new MidiMessage
        {
            Type = MessageTypes.PitchBend,
            Channel = (int)e.Channel,
            Value = e.PitchValue,
            Timestamp = GetTimestamp()
        };
    }

    public static MidiMessage Build(
    NoteOnEvent e,
    string sourceDevice)
    {
        return new MidiMessage
        {
            Type = MessageTypes.NoteOn,
            Channel = (int)e.Channel,
            Note = e.NoteNumber,
            Velocity = e.Velocity,
            Timestamp = GetTimestamp(),
            SourceDevice = sourceDevice
        };
    }

    public static MidiMessage Build(
    NoteOffEvent e,
    string sourceDevice)
    {
        return new MidiMessage
        {
            Type = MessageTypes.NoteOff,
            Channel = (int)e.Channel,
            Note = e.NoteNumber,
            Velocity = e.Velocity,
            Timestamp = GetTimestamp(),
            SourceDevice = sourceDevice
        };
    }

    public static MidiMessage Build(
    ControlChangeEvent e,
    string sourceDevice)
    {
        return new MidiMessage
        {
            Type = MessageTypes.ControlChange,
            Channel = (int)e.Channel,
            Controller = (int)e.ControlNumber,
            Value = e.ControlValue,
            Timestamp = GetTimestamp(),
            SourceDevice = sourceDevice
        };
    }

    public static MidiMessage Build(
    PitchBendEvent e,
    string sourceDevice)
    {
        return new MidiMessage
        {
            Type = MessageTypes.PitchBend,
            Channel = (int)e.Channel,
            Value = e.PitchValue,
            Timestamp = GetTimestamp(),
            SourceDevice = sourceDevice
        };
    }

    private static long GetTimestamp()
    {
        return DateTimeOffset.UtcNow
            .ToUnixTimeMilliseconds();
    }
}