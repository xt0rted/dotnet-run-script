namespace RunScript;

using System.Diagnostics;

internal class CommandRunner : ICommandRunner
{
    private readonly IConsoleWriter _writer;
    private readonly ProcessContext _processContext;
    private readonly bool _captureOutput;
    private readonly CancellationToken _cancellationToken;

    public CommandRunner(
        IConsoleWriter writer,
        ProcessContext processContext,
        bool captureOutput,
        CancellationToken cancellationToken)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _processContext = processContext ?? throw new ArgumentNullException(nameof(processContext));
        _captureOutput = captureOutput;
        _cancellationToken = cancellationToken;
    }

    public async Task<int> RunAsync(string name, string cmd, string[]? args)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        _writer.Banner(name, ArgumentBuilder.ConcatinateCommandAndArgArrayForDisplay(cmd, args));

        using (var process = new Process())
        {
            process.StartInfo.WorkingDirectory = _processContext.WorkingDirectory;
            process.StartInfo.FileName = _processContext.Shell;

            StreamForwarder? outStream = null;
            StreamForwarder? errStream = null;
            Task? taskOut = null;
            Task? taskErr = null;

            if (_captureOutput)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                outStream = new StreamForwarder().Capture();
                errStream = new StreamForwarder().Capture();
            }

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

            if (_captureOutput)
            {
                taskOut = outStream!.BeginReadAsync(process.StandardOutput);
                taskErr = errStream!.BeginReadAsync(process.StandardError);
            }

            await process.WaitForExitAsync(_cancellationToken);

            if (_captureOutput)
            {
                await taskOut!.WaitAsync(_cancellationToken);
                await taskErr!.WaitAsync(_cancellationToken);

                _writer.Line(outStream!.CapturedOutput);
                _writer.Error(errStream!.CapturedOutput);
            }

            return process.ExitCode;
        }
    }
}
