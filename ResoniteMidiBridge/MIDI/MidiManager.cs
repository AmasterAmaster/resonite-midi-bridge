using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using ResoniteMidiBridge.Utilities;

namespace ResoniteMidiBridge.MIDI;

public class MidiManager
{
    private readonly List<InputDevice> _connectedDevices = new();

    private readonly HashSet<string> _knownDeviceNames = new();

    private CancellationTokenSource? _deviceMonitorToken;

    public event EventHandler<NoteOnEvent>? NoteOnReceived;
    public event EventHandler<NoteOffEvent>? NoteOffReceived;
    public event EventHandler<ControlChangeEvent>? ControlChangeReceived;
    public event EventHandler<PitchBendEvent>? PitchBendReceived;

    public IReadOnlyList<InputDevice> ConnectedDevices => _connectedDevices;

    public void Initialize()
    {
        Logger.Info("Scanning MIDI devices...");

        var devices = InputDevice.GetAll();

        foreach (var device in devices)
        {
            Logger.Info($"Found Device: {device.Name}");

            _knownDeviceNames.Add(device.Name);

            RegisterDevice(device);
        }

        Logger.Info(
            $"Detected {_connectedDevices.Count} MIDI input device(s).");

        _deviceMonitorToken = new CancellationTokenSource();

        _ = Task.Run(() =>
            MonitorDevicesAsync(_deviceMonitorToken.Token));
    }

    private void RegisterDevice(InputDevice device)
    {
        if (_connectedDevices.Any(d => d.Name == device.Name))
        {
            return;
        }

        try
        {
            Logger.Info($"Connecting: {device.Name}");

            device.EventReceived += OnMidiEventReceived;
            device.StartEventsListening();

            _connectedDevices.Add(device);

            Logger.Success($"Connected: {device.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error(
                $"Failed to connect to {device.Name}: {ex.Message}");
        }
    }

    private void DisconnectDevice(string deviceName)
    {
        var device = _connectedDevices
            .FirstOrDefault(d => d.Name == deviceName);

        if (device == null)
        {
            return;
        }

        try
        {
            Logger.Warning(
                $"Disconnecting device: {deviceName}");

            device.EventReceived -= OnMidiEventReceived;

            device.Dispose();

            _connectedDevices.Remove(device);
        }
        catch (Exception ex)
        {
            Logger.Error(
                $"Failed to disconnect {deviceName}: {ex.Message}");
        }
    }

    private void OnMidiEventReceived(
        object? sender,
        MidiEventReceivedEventArgs e)
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

    private async Task MonitorDevicesAsync(
        CancellationToken token)
    {
        Logger.Info(
            "Started MIDI device monitor.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                RefreshDevices();
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Device monitor error: {ex.Message}");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        Logger.Info(
            "Stopped MIDI device monitor.");
    }

    private void RefreshDevices()
    {
        var currentDevices = InputDevice
            .GetAll()
            .Select(d => d.Name)
            .ToHashSet();

        var addedDevices = currentDevices
            .Except(_knownDeviceNames)
            .ToList();

        foreach (var added in addedDevices)
        {
            try
            {
                var device =
                    InputDevice.GetByName(added);

                Logger.Success(
                    $"Device Connected: {added}");

                RegisterDevice(device);

                _knownDeviceNames.Add(added);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed to register {added}: {ex.Message}");
            }
        }

        var removedDevices = _knownDeviceNames
            .Except(currentDevices)
            .ToList();

        foreach (var removed in removedDevices)
        {
            Logger.Warning(
                $"Device Disconnected: {removed}");

            DisconnectDevice(removed);

            _knownDeviceNames.Remove(removed);
        }
    }

    public void Shutdown()
    {
        Logger.Info(
            "Shutting down MIDI devices...");

        _deviceMonitorToken?.Cancel();

        foreach (var device in _connectedDevices.ToList())
        {
            try
            {
                device.EventReceived -= OnMidiEventReceived;

                device.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Error during device cleanup: {ex.Message}");
            }
        }

        _connectedDevices.Clear();
        _knownDeviceNames.Clear();

        Logger.Info(
            "MIDI shutdown completed.");
    }
}