using System.Windows.Media;

namespace PythonEnvBuilder.Models;

public sealed class LogEntry
{
    public LogEntry(string kind, string message, Brush color)
    {
        Time = DateTime.Now.ToString("HH:mm:ss");
        Kind = kind;
        Message = message;
        Color = color;
    }

    public string Time { get; }
    public string Kind { get; }
    public string Message { get; }
    public Brush Color { get; }
}
