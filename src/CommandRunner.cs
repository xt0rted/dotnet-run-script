namespace RunScript;

using System.Diagnostics;

internal class CommandRunner : ICommandRunner
{
    private readonly IConsoleWriter _writer;
    private readonly ProcessContext _processContext;
    private readonly CancellationToken _cancellationToken;

    public CommandRunner(
        IConsoleWriter writer,
        ProcessContext processContext,
        CancellationToken cancellationToken)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _processContext = processContext ?? throw new ArgumentNullException(nameof(processContext));
        _cancellationToken = cancellationToken;
    }

    public async Task<int> RunAsync(string name, string cmd, string[]? args)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        _writer.Banner(name, ArgumentBuilder.ConcatinateCommandAndArgArrayForDisplay(cmd, args));
        _writer.LineVerbose("Using shell: {0}", _processContext.Shell);
        _writer.BlankLineVerbose();

        using (var process = new Process())
        {
            process.StartInfo.WorkingDirectory = _processContext.WorkingDirectory;
            process.StartInfo.FileName = _processContext.Shell;

            if (_processContext.IsCmd)
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
