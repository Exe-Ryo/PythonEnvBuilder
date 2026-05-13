using System.Diagnostics;
using PythonEnvBuilder.Models;

namespace PythonEnvBuilder.Services;

public sealed class ShellService
{
    public async Task<CommandResult> RunAsync(string command, string workingDirectory, CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c " + command,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);

        return new CommandResult(command, process.ExitCode, await stdoutTask, await stderrTask);
    }

    public Task RestartAsync()
    {
        return Task.CompletedTask;
    }
}
