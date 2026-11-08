using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace ResoniteMidiBridge.MIDI;

public class MidiManager
{
    private readonly List<InputDevice> _connectedDevices = new();

    public event EventHandler<NoteOnEvent>? NoteOnReceived;
    public event EventHandler<NoteOffEvent>? NoteOffReceived;
    public event EventHandler<ControlChangeEvent>? ControlChangeReceived;
    public event EventHandler<PitchBendEvent>? PitchBendReceived;

    public IReadOnlyList<InputDevice> ConnectedDevices => _connectedDevices;

    public void Initialize()
    {
        Console.WriteLine("Scanning MIDI devices...\n");

        var devices = InputDevice.GetAll();

        foreach (var device in devices)
        {
            RegisterDevice(device);
        }

        Console.WriteLine($"\nDetected {_connectedDevices.Count} MIDI input device(s).");
    }

    private void RegisterDevice(InputDevice device)
    {
        try
        {
            Console.WriteLine($"Connecting: {device.Name}");

            device.EventReceived += OnMidiEventReceived;
            device.StartEventsListening();

            _connectedDevices.Add(device);

            Console.WriteLine($"Connected: {device.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to {device.Name}");
            Console.WriteLine(ex.Message);
        }
    }

    private void OnMidiEventReceived(object? sender, MidiEventReceivedEventArgs e)
    {
        switch (e.Event)
        {
            case NoteOnEvent noteOn:
                NoteOnReceived?.Invoke(this, noteOn);
                break;

            case NoteOffEvent noteOff:
                NoteOffReceived?.Invoke(this, noteOff);
                break;

            case ControlChangeEvent controlChange:
                ControlChangeReceived?.Invoke(this, controlChange);
                break;

            case PitchBendEvent pitchBend:
                PitchBendReceived?.Invoke(this, pitchBend);
                break;
        }
    }

    public void Shutdown()
    {
        Console.WriteLine("Shutting down MIDI devices...");

        foreach (var device in _connectedDevices)
        {
            try
            {
                device.EventReceived -= OnMidiEventReceived;
                device.Dispose();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _connectedDevices.Clear();
    }
}
