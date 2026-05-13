using PythonEnvBuilder.Models;

namespace PythonEnvBuilder.Services;

public sealed class EnvironmentDiagnosticService
{
    private readonly CommandCatalog _commands;
    private readonly ShellService _shell;

    public EnvironmentDiagnosticService(CommandCatalog commands, ShellService shell)
    {
        _commands = commands;
        _shell = shell;
    }

    public async Task<IReadOnlyList<(DiagnosticItem Item, CommandResult Result)>> RunAsync(string workingDirectory)
    {
        var checks = new[]
        {
            ("Python", "Python Installed", _commands.Get("PythonVersion")),
            ("py launcher", "py launcher detected", _commands.Get("PyVersion")),
            ("PATH", "Python PATH", _commands.Get("WherePython")),
            ("VSCode", "VSCode detected", _commands.Get("WhereCode"))
        };

        var results = new List<(DiagnosticItem, CommandResult)>();
        foreach (var check in checks)
        {
            var result = await _shell.RunAsync(check.Item3, workingDirectory);
            var status = result.IsSuccess ? "[OK]" : "[WARN]";
            var detail = result.IsSuccess ? check.Item2 : "not detected";
            results.Add((new DiagnosticItem(check.Item1, status, detail), result));
        }

        return results;
    }
}
