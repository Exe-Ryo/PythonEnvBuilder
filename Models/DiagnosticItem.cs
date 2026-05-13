namespace PythonEnvBuilder.Models;

public sealed class DiagnosticItem
{
    public DiagnosticItem(string name, string status, string detail)
    {
        Name = name;
        Status = status;
        Detail = detail;
    }

    public string Name { get; }
    public string Status { get; }
    public string Detail { get; }
}
