using System.IO;
using System.Text.Json;

namespace PythonEnvBuilder.Services;

public sealed class CommandCatalog
{
    private readonly Dictionary<string, string> _commands = new()
    {
        ["PythonVersion"] = "python --version",
        ["PyVersion"] = "py --version",
        ["WherePython"] = "where python",
        ["WhereCode"] = "where code",
        ["CreateVenv"] = "python -m venv {venv_name}",
        ["UpgradePip"] = "{venv_python} -m pip install --upgrade pip",
        ["InstallPackage"] = "{venv_python} -m pip install {package}",
        ["FreezeRequirements"] = "{venv_python} -m pip freeze",
        ["InstallRequirements"] = "{venv_python} -m pip install -r requirements.txt",
        ["OpenVSCode"] = "code ."
    };

    public CommandCatalog()
    {
        LoadOverride(Path.Combine(AppContext.BaseDirectory, ".pyenvbuilder", "commands.json"));
        LoadOverride(Path.Combine(Environment.CurrentDirectory, ".pyenvbuilder", "commands.json"));
    }

    public string Get(string key, IReadOnlyDictionary<string, string>? values = null)
    {
        var command = _commands[key];
        if (values is null)
        {
            return command;
        }

        foreach (var item in values)
        {
            command = command.Replace("{" + item.Key + "}", item.Value);
        }

        return command;
    }

    private void LoadOverride(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var json = File.ReadAllText(path);
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (values is null)
        {
            return;
        }

        foreach (var item in values)
        {
            _commands[item.Key] = item.Value;
        }
    }
}
