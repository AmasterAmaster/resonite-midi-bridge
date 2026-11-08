using System.Text.Json;

namespace ResoniteMidiBridge.Messages;

public class MidiMessage
{
    public MessageTypes Type { get; set; }

    public int Channel { get; set; }

    public int? Note { get; set; }

    public int? Velocity { get; set; }

    public int? Controller { get; set; }

    public int? Value { get; set; }

    public long Timestamp { get; set; }

    public string SourceDevice { get; set; } = string.Empty;

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static MidiMessage? FromJson(string json)
    {
        return JsonSerializer.Deserialize<MidiMessage>(json);
    }
}