using ResoniteMidiBridge.MIDI;
using ResoniteMidiBridge.Networking;

Console.WriteLine("=== Resonite MIDI Bridge ===");

var midiManager = new MidiManager();
var midiHandler = new MidiEventHandler();
var wsServer =
    new WebSocketServer("http://localhost:8080/ws/");

midiHandler.Subscribe(midiManager);

midiManager.Initialize();

_ = Task.Run(async () =>
{
    await wsServer.StartAsync();
});

Console.WriteLine("Listening for MIDI events...");
Console.WriteLine("Press ENTER to quit.\n");

Console.ReadLine();

wsServer.Stop();
midiManager.Shutdown();