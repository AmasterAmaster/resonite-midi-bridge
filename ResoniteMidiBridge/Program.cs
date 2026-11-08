using ResoniteMidiBridge.MIDI;

Console.WriteLine("=== Resonite MIDI Bridge ===");

var midiManager = new MidiManager();
var midiHandler = new MidiEventHandler();

midiHandler.Subscribe(midiManager);

midiManager.Initialize();

Console.WriteLine("Listening for MIDI events...");
Console.WriteLine("Press ENTER to quit.\n");

Console.ReadLine();

midiManager.Shutdown();