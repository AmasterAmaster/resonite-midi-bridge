using ResoniteMidiBridge.Utilities;

namespace ResoniteMidiBridge.UI;

public static class StartupMenu
{
    public static StartupMode Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=================================");
            Console.WriteLine("      Resonite MIDI Bridge");
            Console.WriteLine("=================================");
            Console.WriteLine();
            Console.WriteLine("Select Startup Mode:");
            Console.WriteLine();
            Console.WriteLine("  [1] Live MIDI Device");
            Console.WriteLine("  [2] MIDI File Playback");
            Console.WriteLine();
            Console.WriteLine("  [0] Exit");
            Console.WriteLine();
            Console.Write("Selection: ");

            string? input = Console.ReadLine();

            switch (input?.Trim())
            {
                case "0":
                    Logger.Info("Selected Exit");
                    return StartupMode.Exit;

                case "1":
                    Logger.Info("Selected Live Mode");
                    return StartupMode.Live;

                case "2":
                    Logger.Info("Selected File Mode");
                    return StartupMode.File;

                default:
                    Logger.Warning(
                        "Invalid selection. Please choose 1 or 2.");

                    Console.WriteLine();
                    Console.WriteLine(
                        "Press ENTER to continue...");
                    Console.ReadLine();

                    break;
            }
        }
    }
}