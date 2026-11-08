using Melanchall.DryWetMidi.Core;
using ResoniteMidiBridge.Messages;
using ResoniteMidiBridge.Networking;
using ResoniteMidiBridge.Utilities;

namespace ResoniteMidiBridge.MIDI;

public class MidiEventHandler
{
    private readonly WebSocketServer _webSocketServer;

    public MidiEventHandler(WebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
    }

    public void Subscribe(MidiManager midiManager)
    {
        midiManager.NoteOnReceived += HandleNoteOn;
        midiManager.NoteOffReceived += HandleNoteOff;
        midiManager.ControlChangeReceived += HandleControlChange;
        midiManager.PitchBendReceived += HandlePitchBend;

        Logger.Info("MIDI event subscriptions established.");
    }

    private void HandleNoteOn(object? sender, NoteOnEvent e)
    {
        var message = MidiMessageBuilder.Build(
            e,
            "UnknownDevice");

        SendMessage(message);
    }

    private void HandleNoteOff(object? sender, NoteOffEvent e)
    {
        var message = MidiMessageBuilder.Build(
            e,
            "UnknownDevice");

        SendMessage(message);
    }

    private void HandleControlChange(object? sender, ControlChangeEvent e)
    {
        var message = MidiMessageBuilder.Build(
            e,
            "UnknownDevice");

        SendMessage(message);
    }

    private void HandlePitchBend(object? sender, PitchBendEvent e)
    {
        var message = MidiMessageBuilder.Build(
            e,
            "UnknownDevice");

        SendMessage(message);
    }

    private void SendMessage(MidiMessage message)
    {
        string json = message.ToJson();

        Logger.Debug($"MIDI => {json}");

        _ = Task.Run(async () =>
        {
            try
            {
                await _webSocketServer.BroadcastAsync(json);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed to broadcast MIDI message: {ex.Message}");
            }
        });
    }
}