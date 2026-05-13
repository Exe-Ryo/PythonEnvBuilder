using PythonEnvBuilder.Models;
using System.IO;

namespace PythonEnvBuilder.Services;

public sealed class PipService
{
    private readonly CommandCatalog _commands;
    private readonly ShellService _shell;

    public PipService(CommandCatalog commands, ShellService shell)
    {
        _commands = commands;
        _shell = shell;
    }

    public string VenvPython(string venvName) => Path.Combine(venvName, "Scripts", "python.exe");

    public Task<CommandResult> UpgradeAsync(string venvName, string workingDirectory)
    {
        return _shell.RunAsync(Format("UpgradePip", venvName), workingDirectory);
    }

    public Task<CommandResult> InstallPackageAsync(string venvName, string package, string workingDirectory)
    {
        return _shell.RunAsync(Format("InstallPackage", venvName, ("package", package)), workingDirectory);
    }

    public async Task<CommandResult> ExportRequirementsAsync(string venvName, string workingDirectory)
    {
        var result = await _shell.RunAsync(Format("FreezeRequirements", venvName), workingDirectory);
        if (result.IsSuccess)
        {
            await File.WriteAllTextAsync(Path.Combine(workingDirectory, "requirements.txt"), result.StandardOutput);
        }

        return result;
    }

    public Task<CommandResult> ImportRequirementsAsync(string venvName, string workingDirectory)
    {
        return _shell.RunAsync(Format("InstallRequirements", venvName), workingDirectory);
    }

    private string Format(string key, string venvName, params (string Key, string Value)[] values)
    {
        var replacements = new Dictionary<string, string> { ["venv_python"] = VenvPython(venvName) };
        foreach (var value in values)
        {
            replacements[value.Key] = value.Value;
        }

        return _commands.Get(key, replacements);
    }
}
