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

    private readonly Project _project;
    private readonly string _workingDirectory;
    private readonly IFormatProvider _consoleFormatProvider;

    private readonly ImmutableArray<string> _scriptNames;

    public RunScriptCommand(
        string name,
        string? description,
        Project project,
        string workingDirectory,
        IFormatProvider consoleFormatProvider)
        : base(name, description)
    {
        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));

        _project = project ?? throw new ArgumentNullException(nameof(project));
        _workingDirectory = workingDirectory;
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
            var args = script == Name
                ? context.ParseResult.UnparsedTokens
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
        shell ??= _isWindows
            ? _comspec ?? "cmd"
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
        IReadOnlyList<string>? args,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        writer.Banner(name, CommandBannerText(cmd, args));
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
                    cmd,
                    " ",
                    ArgumentEscaper.EscapeAndConcatenateArgArrayForCmdProcessStart(args),
                    "\"");
            }
            else
            {
                process.StartInfo.ArgumentList.Add("-c");
                process.StartInfo.ArgumentList.Add(
                    string.Concat(
                        cmd.AsSpan(),
                        " ",
                        ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(args)));
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

    private static string CommandBannerText(string? cmd, IReadOnlyList<string>? args)
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);

        sb.Append(cmd);

        if (args?.Count > 0)
        {
            for (var i = 0; i < args.Count; i++)
            {
                sb.Append(' ');
                sb.Append(args[i]);
            }
        }

        return sb.ToString();
    }
}
