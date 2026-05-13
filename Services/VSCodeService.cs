using System.IO;
using System.Text.Json;
using PythonEnvBuilder.Models;

namespace PythonEnvBuilder.Services;

public sealed class VSCodeService
{
    private readonly CommandCatalog _commands;
    private readonly ShellService _shell;

    public VSCodeService(CommandCatalog commands, ShellService shell)
    {
        _commands = commands;
        _shell = shell;
    }

    public async Task WriteSettingsAsync(string workingDirectory, string venvName)
    {
        var vscodeDirectory = Path.Combine(workingDirectory, ".vscode");
        Directory.CreateDirectory(vscodeDirectory);

        var settings = new Dictionary<string, string>
        {
            ["python.defaultInterpreterPath"] = Path.Combine(venvName, "Scripts", "python.exe")
        };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(vscodeDirectory, "settings.json"), json);
    }

    public Task<CommandResult> OpenAsync(string workingDirectory)
    {
        return _shell.RunAsync(_commands.Get("OpenVSCode"), workingDirectory);
    }
}
