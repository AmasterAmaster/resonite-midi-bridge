namespace ResoniteMidiBridge.Utilities;

public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static void Info(string message)
    {
        Write("INFO", ConsoleColor.Cyan, message);
    }

    public static void Success(string message)
    {
        Write("SUCCESS", ConsoleColor.Green, message);
    }

    public static void Warning(string message)
    {
        Write("WARNING", ConsoleColor.Yellow, message);
    }

    public static void Error(string message)
    {
        Write("ERROR", ConsoleColor.Red, message);
    }

    public static void Debug(string message)
    {
        Write("DEBUG", ConsoleColor.Magenta, message);
    }

    private static void Write(
        string level,
        ConsoleColor color,
        string message)
    {
        var timestamp =
            DateTime.Now.ToString("HH:mm:ss");

        Console.ForegroundColor = color;

        Console.WriteLine(
            $"[{timestamp}] [{level}] {message}");

        Console.ResetColor();
    }
}