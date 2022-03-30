namespace RunScript;

internal sealed class ProcessContext
{
    private ProcessContext(string shell, bool isCmd, string workingDirectory)
    {
        Shell = shell;
        IsCmd = isCmd;
        WorkingDirectory = workingDirectory;
    }

    public string Shell { get; }

    public bool IsCmd { get; }

    public string WorkingDirectory { get; }

    public static ProcessContext Create(string shell, bool isCmd, string workingDirectory)
        => new(shell, isCmd, workingDirectory);
}
