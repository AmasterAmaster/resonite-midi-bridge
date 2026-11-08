namespace ResoniteMidiBridge.MIDI;

public class MidiDeviceInfo
{
    public string Name { get; set; } = string.Empty;

    public string DeviceId { get; set; } = Guid.NewGuid().ToString();

    public bool IsConnected { get; set; }

    public DateTime ConnectedAt { get; set; }
        = DateTime.UtcNow;

    public override string ToString()
    {
        return
            $"Device(Name={Name}, " +
            $"Id={DeviceId}, " +
            $"Connected={IsConnected})";
    }
}