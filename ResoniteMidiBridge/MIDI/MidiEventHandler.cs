using Melanchall.DryWetMidi.Core;

namespace ResoniteMidiBridge.MIDI;

public class MidiEventHandler
{
    public void Subscribe(MidiManager midiManager)
    {
        midiManager.NoteOnReceived += HandleNoteOn;
        midiManager.NoteOffReceived += HandleNoteOff;
        midiManager.ControlChangeReceived += HandleControlChange;
        midiManager.PitchBendReceived += HandlePitchBend;
    }

    private void HandleNoteOn(object? sender, NoteOnEvent e)
    {
        var message = new MidiMessage
        {
            Type = MessageTypes.NoteOn,
            Channel = (int)e.Channel,
            Note = e.NoteNumber,
            Velocity = e.Velocity,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        Console.WriteLine(message.ToJson());
    }

    private void HandleNoteOff(object? sender, NoteOffEvent e)
    {
        Console.WriteLine(
            $"[NOTE OFF] Channel:{e.Channel} Note:{e.NoteNumber}");
    }

    private void HandleControlChange(object? sender, ControlChangeEvent e)
    {
        Console.WriteLine(
            $"[CC] Channel:{e.Channel} Controller:{e.ControlNumber} Value:{e.ControlValue}");
    }

    private void HandlePitchBend(object? sender, PitchBendEvent e)
    {
        Console.WriteLine(
            $"[PITCH BEND] Channel:{e.Channel} Value:{e.PitchValue}");
    }
}