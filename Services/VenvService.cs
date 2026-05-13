using PythonEnvBuilder.Models;

namespace PythonEnvBuilder.Services;

public sealed class VenvService
{
    private readonly CommandCatalog _commands;
    private readonly ShellService _shell;

    public VenvService(CommandCatalog commands, ShellService shell)
    {
        _commands = commands;
        _shell = shell;
    }

    public Task<CommandResult> CreateAsync(string venvName, string workingDirectory)
    {
        return _shell.RunAsync(_commands.Get("CreateVenv", new Dictionary<string, string> { ["venv_name"] = venvName }), workingDirectory);
    }
}
