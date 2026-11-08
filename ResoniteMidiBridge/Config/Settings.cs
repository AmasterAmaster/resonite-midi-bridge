namespace ResoniteMidiBridge.Config;

public sealed class Settings
{
    private static readonly Lazy<Settings> _instance =
        new(() => new Settings());

    public static Settings Instance => _instance.Value;

    private Settings()
    {
    }

    public string WebSocketHost { get; set; } = "localhost";

    public int WebSocketPort { get; set; } = 8080;

    public string WebSocketPath { get; set; } = "ws/";

    public bool EnableConsoleLogging { get; set; } = true;

    public bool EnableDebugLogging { get; set; } = true;

    public bool AutoConnectDevices { get; set; } = true;

    public bool EnableDeviceHotplugging { get; set; } = false;

    public bool EnableTempoSync { get; set; } = false;

    public string GetWebSocketUrl()
    {
        return $"http://{WebSocketHost}:{WebSocketPort}/{WebSocketPath}";
    }
}