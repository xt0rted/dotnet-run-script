namespace RunScript;

using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text.RegularExpressions;

internal class RunScriptCommand : RootCommand, ICommandHandler
{
    // This is the same regex used by npm's run-script library
    private static readonly Regex _isCmdCheck = new("(?:^|\\\\)cmd(?:\\.exe)?$", RegexOptions.IgnoreCase);

    private readonly IEnvironment _environment;
    private readonly IFormatProvider _consoleFormatProvider;
    private string _workingDirectory;

    public RunScriptCommand(
        IEnvironment environment,
        IFormatProvider consoleFormatProvider,
        string workingDirectory)
        : base("Run arbitrary project scripts")
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _consoleFormatProvider = consoleFormatProvider ?? throw new ArgumentNullException(nameof(consoleFormatProvider));

        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));

        _workingDirectory = workingDirectory;

        AddArgument(GlobalArguments.Scripts);

        AddOption(GlobalOptions.IfPresent);
        AddOption(GlobalOptions.ScriptShell);
        AddOption(GlobalOptions.Verbose);

        Handler = this;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var ifPresent = context.ParseResult.GetValueForOption(GlobalOptions.IfPresent);
        var scriptShell = context.ParseResult.GetValueForOption(GlobalOptions.ScriptShell);
        var verbose = context.ParseResult.GetValueForOption(GlobalOptions.Verbose);
        var scripts = context.ParseResult.GetValueForArgument(GlobalArguments.Scripts);

        IConsoleWriter writer = new ConsoleWriter(context.Console, _consoleFormatProvider, verbose);

        writer.VerboseBanner();

        Project? project;
        try
        {
            _environment.SetEnvironmentVariable("INIT_CWD", _workingDirectory);

            (project, _workingDirectory) = await new ProjectLoader().LoadAsync(_workingDirectory);
        }
        catch (Exception ex)
        {
            writer.Error(ex.Message);

            return 1;
        }

        (scriptShell, var isCmd) = GetScriptShell(scriptShell ?? project.ScriptShell);

        writer.LineVerbose("Using shell: {0}", scriptShell);
        writer.BlankLine();

        if (scripts.Length == 0)
        {
            PrintAvailableScripts(writer, project.Scripts!);

            return 0;
        }

        // The `env` script is special so if it's not explicitly declared we act like it was
        var scriptsToRun = scripts
            .Select(script => (name: script, exists: project.Scripts!.ContainsKey(script) || script == "env"))
            .ToList();

        // When `--if-present` isn't specified and a script wasn't found in the config then we show an error and stop
        if (scriptsToRun.Any(s => !s.exists) && !ifPresent)
        {
            writer.Error(
                "Script not found: {0}",
                string.Join(
                    ", ",
                    scriptsToRun
                        .Where(script => !script.exists)
                        .Select(script => script.name)));

            return 1;
        }

        var runResults = new List<(string scriptName, int exitCode)>();

        foreach (var (scriptName, scriptExists) in scriptsToRun)
        {
            if (!scriptExists)
            {
                writer.Banner($"Skipping script {scriptName}");

                continue;
            }

            // UnparsedTokens is backed by string[] so if we cast
            // back to that we get a lot better perf down the line.
            // Hopefully this doesn't break in the future 🤞
            var scriptArgs = (string[])context.ParseResult.UnparsedTokens;

            var result = await ProcessScriptAsync(
                project.Scripts!,
                writer,
                scriptShell!,
                isCmd,
                scriptName,
                scriptArgs,
                context.GetCancellationToken());

            runResults.Add((scriptName, result));

            if (result != 0)
            {
                break;
            }
        }

        return LogErrors(writer, runResults);
    }

    private static int LogErrors(IConsoleWriter writer, List<(string scriptName, int exitCode)> results)
    {
        // If only 1 script ran we don't need a report of the results
        if (results.Count == 1)
        {
            return results[0].exitCode;
        }

        var hadError = false;

        foreach (var (scriptName, exitCode) in results.Where(r => r.exitCode != 0))
        {
            hadError = true;

            writer.Line(
                "ERROR: \"{0}\" exited with {1}",
                writer.ColorText(ConsoleColor.Blue, scriptName),
                writer.ColorText(ConsoleColor.Green, exitCode));
        }

        return hadError ? 1 : 0;
    }

    /// <summary>
    /// The help command that lists all the scripts availble in the <g>global.json</g>.
    /// </summary>
    /// <param name="writer">The console logger instance to use.</param>
    /// <param name="scripts">The project's scripts.</param>
    private static void PrintAvailableScripts(IConsoleWriter writer, IDictionary<string, string?> scripts)
    {
        writer.Line("Available via `{0}`:", writer.ColorText(ConsoleColor.Blue, "dotnet r"));
        writer.BlankLine();

        foreach (var script in scripts.Keys)
        {
            writer.Line("  {0}", script);
            writer.SecondaryLine("    {0}", scripts[script]);
            writer.BlankLine();
        }
    }

    /// <summary>
    /// Custom "script" that lists all available environment variables that will be available to the executing scripts.
    /// </summary>
    /// <param name="writer">The console logger instance to use.</param>
    private void PrintEnvironmentVariables(IConsoleWriter writer)
    {
        writer.Banner("env");

        foreach (var (key, value) in environmentVariables(_environment).OrderBy(v => v.key, StringComparer.InvariantCulture))
        {
            writer.Line("{0}={1}", key, value);
        }

        static IEnumerable<(string key, string value)> environmentVariables(IEnvironment environment)
        {
            var variables = environment.GetEnvironmentVariables();

            foreach (var key in variables.Keys)
            {
                yield return new((string)key!, (string)variables[key!]!);
            }
        }
    }

    /// <summary>
    /// Gets the script shell to use.
    /// </summary>
    /// <param name="shell">A optional custom shell to use instead of the system default.</param>
    /// <returns>The shell to use and if it's <c>cmd</c> or not.</returns>
    private (string shell, bool isCmd) GetScriptShell(string? shell)
    {
        shell ??= _environment.IsWindows
            ? _environment.GetEnvironmentVariable("COMSPEC") ?? "cmd"
            : "sh";

        var isCmd = _isCmdCheck.IsMatch(shell);

        return (shell, isCmd);
    }

    /// <summary>
    /// Process a script and it's pre & post scripts.
    /// </summary>
    /// <param name="scripts">The project's scripts.</param>
    /// <param name="writer">The console logger instance to use.</param>
    /// <param name="scriptShell">The shell to run the script in.</param>
    /// <param name="isCmd">If the shell is <c>cmd</c> or not.</param>
    /// <param name="script"></param>
    /// <param name="scriptArgs">
    /// Any arguments to pass to the executing script.
    /// Will not be passed to the <c>pre</c> or <c>post</c> scripts.
    /// </param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns><c>0</c> if there were no errors; otherwise the exit code of the failed script.</returns>
    private async Task<int> ProcessScriptAsync(
        IDictionary<string, string?> scripts,
        IConsoleWriter writer,
        string scriptShell,
        bool isCmd,
        string script,
        string[]? scriptArgs,
        CancellationToken cancellationToken)
    {
        var scriptNames = ImmutableArray.Create(new[] { "pre" + script, script, "post" + script });

        foreach (var subScript in scriptNames.Where(scriptName => scripts.ContainsKey(scriptName) || scriptName == "env"))
        {
            // At this point we should have done enough checks to make sure the only not found script is `env`
            if (!scripts.ContainsKey(subScript))
            {
                PrintEnvironmentVariables(writer);

                continue;
            }

            var args = subScript == script
                ? scriptArgs
                : null;

            var result = await RunScriptAsync(
                writer,
                subScript,
                scripts[subScript],
                scriptShell,
                isCmd,
                args,
                cancellationToken);

            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }

    /// <summary>
    /// Execute the script in another process.
    /// </summary>
    /// <param name="writer">The console logger instance to use.</param>
    /// <param name="name">The name of the script to run.</param>
    /// <param name="cmd">The contents of the script to run.</param>
    /// <param name="shell">The shell to run the script in.</param>
    /// <param name="isCmd">If the shell is <c>cmd</c> or not.</param>
    /// <param name="args">Any arguments to pass to the executing script.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The exit code of the script./returns>
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
