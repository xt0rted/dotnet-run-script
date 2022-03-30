namespace RunScript;

using System.Diagnostics;

internal class CommandRunner : ICommandRunner
{
    private readonly IConsoleWriter _writer;
    private readonly string _workingDirectory;
    private readonly string _shell;
    private readonly bool _isCmd;
    private readonly CancellationToken _cancellationToken;

    public CommandRunner(
        IConsoleWriter writer,
        string workingDirectory,
        string shell,
        bool isCmd,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));
        if (string.IsNullOrEmpty(shell)) throw new ArgumentException($"'{nameof(shell)}' cannot be null or empty.", nameof(shell));

        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _workingDirectory = workingDirectory;
        _shell = shell;
        _isCmd = isCmd;
        _cancellationToken = cancellationToken;
    }

    public async Task<int> RunAsync(string name, string cmd, string[]? args)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        _writer.Banner(name, ArgumentBuilder.ConcatinateCommandAndArgArrayForDisplay(cmd, args));
        _writer.LineVerbose("Using shell: {0}", _shell);
        _writer.BlankLineVerbose();

        using (var process = new Process())
        {
            process.StartInfo.WorkingDirectory = _workingDirectory;
            process.StartInfo.FileName = _shell;

            if (_isCmd)
            {
                process.StartInfo.Arguments = string.Concat(
                    "/d /s /c \"",
                    ArgumentBuilder.EscapeAndConcatenateCommandAndArgArrayForCmdProcessStart(cmd, args),
                    "\"");
            }
            else
            {
                process.StartInfo.ArgumentList.Add("-c");
                process.StartInfo.ArgumentList.Add(ArgumentBuilder.EscapeAndConcatenateCommandAndArgArrayForProcessStart(cmd, args));
            }

            process.Start();

#if NET5_0_OR_GREATER
            await process.WaitForExitAsync(_cancellationToken);
#else
            await Task.CompletedTask;

            process.WaitForExit();
#endif

            return process.ExitCode;
        }
    }
}
