namespace RunScript;

using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text.RegularExpressions;

internal class RunScriptCommand : Command, ICommandHandler
{
    private static readonly Regex _isCmdCheck = new("(?:^|\\\\)cmd(?:\\.exe)?$", RegexOptions.IgnoreCase);

    private readonly Project _project;
    private readonly string _workingDirectory;
    private readonly IEnvironment _environment;
    private readonly IFormatProvider _consoleFormatProvider;

    private readonly ImmutableArray<string> _scriptNames;

    public RunScriptCommand(
        string name,
        string? description,
        Project project,
        string workingDirectory,
        IEnvironment environment,
        IFormatProvider consoleFormatProvider)
        : base(name, description)
    {
        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));

        _project = project ?? throw new ArgumentNullException(nameof(project));
        _workingDirectory = workingDirectory;
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _consoleFormatProvider = consoleFormatProvider ?? throw new ArgumentNullException(nameof(consoleFormatProvider));

        _scriptNames = ImmutableArray.Create(new[] { "pre" + Name, Name, "post" + Name });

        Handler = this;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var writer = new ConsoleWriter(context.Console, _consoleFormatProvider, context.ParseResult.GetValueForOption(GlobalOptions.Verbose));

        writer.VerboseBanner();

        var scriptShell = context.ParseResult.GetValueForOption(GlobalOptions.ScriptShell);
        var (shell, isCmd) = GetScriptShell(scriptShell ?? _project.ScriptShell);

        var ct = context.GetCancellationToken();

        foreach (var script in _scriptNames.Where(scriptName => _project.Scripts!.ContainsKey(scriptName)))
        {
            // UnparsedTokens is backed by string[] so if we cast
            // back to that we get a lot better perf down the line.
            // Hopefully this doesn't break in the future 🤞
            var args = script == Name
                ? (string[])context.ParseResult.UnparsedTokens
                : null;

            var result = await RunScriptAsync(
                writer,
                script,
                _project.Scripts![script],
                shell,
                isCmd,
                args,
                ct);

            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }

    private (string shell, bool isCmd) GetScriptShell(string? shell)
    {
        shell ??= _environment.IsWindows
            ? _environment.GetEnvironmentVariable("COMSPEC") ?? "cmd"
            : "sh";

        var isCmd = _isCmdCheck.IsMatch(shell);

        return (shell, isCmd);
    }

    private async Task<int> RunScriptAsync(
        IConsoleWriter writer,
        string name,
        string? cmd,
        string shell,
        bool isCmd,
        string[]? args,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        writer.Banner(name, ArgumentBuilder.ConcatinateCommandAndArgArrayForDisplay(cmd, args));
        writer.LineVerbose("Using shell: {0}", shell);
        writer.BlankLineVerbose();

        using (var process = new Process())
        {
            process.StartInfo.WorkingDirectory = _workingDirectory;
            process.StartInfo.FileName = shell;

            if (isCmd)
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
            await process.WaitForExitAsync(cancellationToken);
#else
            await Task.CompletedTask;

            process.WaitForExit();
#endif

            return process.ExitCode;
        }
    }
}
