namespace PythonEnvBuilder.Models;

public sealed class CommandResult
{
    public CommandResult(string command, int exitCode, string standardOutput, string standardError)
    {
        Command = command;
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    public string Command { get; }
    public int ExitCode { get; }
    public string StandardOutput { get; }
    public string StandardError { get; }
    public bool IsSuccess => ExitCode == 0;
}
