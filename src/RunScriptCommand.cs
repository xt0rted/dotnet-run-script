namespace RunScript;

using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class RunScriptCommand : Command, ICommandHandler
{
    private static readonly Regex _isCmdCheck = new("(?:^|\\\\)cmd(?:\\.exe)?$", RegexOptions.IgnoreCase);

    private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private readonly string? _comspec = Environment.GetEnvironmentVariable("COMSPEC");

    private string? _projectScriptshell;
    private readonly IDictionary<string, string?> _scripts;
    private readonly string _workingDirectory;

    private readonly ImmutableArray<string> _scriptNames;

    public RunScriptCommand(
        string name,
        string? description,
        string? projectScriptShell,
        IDictionary<string, string?> scripts,
        string workingDirectory)
        : base(name, description)
    {
        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));

        _projectScriptshell = projectScriptShell;
        _scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
        _workingDirectory = workingDirectory;

        _scriptNames = ImmutableArray.Create(new[] { "pre" + Name, Name, "post" + Name });

        Handler = this;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var console = new ConsoleWriter(context.Console, context.ParseResult.GetValueForOption(GlobalOptions.Verbose));
        console.AlertAboutVerbose();

        var scriptShell = context.ParseResult.GetValueForOption(GlobalOptions.ScriptShell);
        if (scriptShell is not null)
        {
            _projectScriptshell = scriptShell;
        }

        var (shell, isCmd) = GetScriptShell();

        var ct = context.GetCancellationToken();

        foreach (var script in _scriptNames.Where(scriptName => _scripts.ContainsKey(scriptName)).ToImmutableArray())
        {
            var args = script == Name
                ? context.ParseResult.UnparsedTokens
                : null;

            var result = await RunScriptAsync(
                console,
                script,
                _scripts[script],
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

    private (string shell, bool isCmd) GetScriptShell()
    {
        var shell = _projectScriptshell;

        shell ??= _isWindows
            ? _comspec ?? "cmd"
            : "sh";

        var isCmd = _isCmdCheck.IsMatch(shell);

        return (shell, isCmd);
    }

    private async Task<int> RunScriptAsync(
        ConsoleWriter console,
        string name,
        string? cmd,
        string shell,
        bool isCmd,
        IReadOnlyList<string>? args,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var toExecute = cmd;
        if (args?.Count > 0)
        {
            toExecute += " ";
            toExecute += string.Join(" ", args);
        }

        console.Banner(name, toExecute);
        console.VerboseLine("Using shell: {0}", shell);
        console.BlankVerboseLine();

        using (var process = new Process())
        {
            process.StartInfo.WorkingDirectory = _workingDirectory;
            process.StartInfo.FileName = shell;

            process.StartInfo.Arguments =
                isCmd
                ? $"/d /s /c {toExecute}"
                : $"-c \"{toExecute}\"";

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
