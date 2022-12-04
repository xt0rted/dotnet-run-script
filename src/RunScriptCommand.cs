namespace RunScript;

using System.CommandLine.Invocation;

using DotNet.Globbing;

using RunScript.Logging;

internal class RunScriptCommand : RootCommand, ICommandHandler
{
    private readonly IEnvironment _environment;
    private readonly IFormatProvider _consoleFormatProvider;
    private string _workingDirectory;

    internal RunScriptCommand(
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

    public int Invoke(InvocationContext context)
        => throw new NotImplementedException();

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

        var builder = new CommandBuilder(
            writer,
            _environment,
            project,
            _workingDirectory,
            // For now we just write to the executing shell, later we can opt to write to the log instead
            captureOutput: false);

        builder.SetUpEnvironment(scriptShell);

        if (scripts.Length == 0)
        {
            GlobalCommands.PrintAvailableScripts(writer, project.Scripts!);

            return 0;
        }

        var scriptsToRun = FindScripts(project.Scripts!, scripts);

        // When `--if-present` isn't specified and a script wasn't found in the config then we show an error and stop
        if (scriptsToRun.Any(s => !s.Exists) && !ifPresent)
        {
            writer.Error(
                "Script not found: {0}",
                string.Join(
                    ", ",
                    scriptsToRun
                        .Where(script => !script.Exists)
                        .Select(script => script.Name)));

            return 1;
        }

        var runResults = new List<RunResult>();

        foreach (var script in scriptsToRun)
        {
            using (var logGroup = writer.Group(_environment, script.Name))
            {
                if (!script.Exists)
                {
                    writer.Banner($"Skipping script {script.Name}");

                    continue;
                }

                // UnparsedTokens is backed by string[] so if we cast
                // back to that we get a lot better perf down the line.
                // Hopefully this doesn't break in the future 🤞
                var scriptArgs = (string[])context.ParseResult.UnparsedTokens;

                var scriptRunner = builder.CreateGroupRunner(context.GetCancellationToken());

                var result = await scriptRunner.RunAsync(
                    script.Name,
                    scriptArgs);

                runResults.Add(new(script.Name, result));

                if (result != 0)
                {
                    break;
                }
            }
        }

        return RunResults(writer, runResults);
    }

    internal static List<ScriptResult> FindScripts(
        IDictionary<string, string?> projectScripts,
        string[] scripts)
    {
        var results = new List<ScriptResult>();

        foreach (var script in scripts)
        {
            // The `env` script is special so if it's not explicitly declared we act like it was
            if (projectScripts.ContainsKey(script) || string.Equals(script, "env", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new(script, true));

                continue;
            }

            var hadMatch = false;
            var matcher = Glob.Parse(
                SwapColonAndSlash(script),
                new GlobOptions
                {
                    Evaluation =
                    {
                        CaseInsensitive = true,
                    }
                });

            foreach (var projectScript in projectScripts.Keys)
            {
                if (matcher.IsMatch(SwapColonAndSlash(projectScript).AsSpan()))
                {
                    hadMatch = true;

                    results.Add(new(projectScript, true));
                }
            }

            if (!hadMatch)
            {
                results.Add(new(script, false));
            }
        }

        return results;
    }

    internal static int RunResults(IConsoleWriter writer, List<RunResult> results)
    {
        // If only 1 script ran we don't need a report of the results
        if (results.Count == 1)
        {
            return results[0].ExitCode;
        }

        var hadError = false;

        foreach (var result in results.Where(r => r.ExitCode != 0))
        {
            hadError = true;

            writer.Line(
                "ERROR: \"{0}\" exited with {1}",
                writer.ColorText(ConsoleColor.Blue, result.Name),
                writer.ColorText(ConsoleColor.Green, result.ExitCode));
        }

        return hadError ? 1 : 0;
    }

    internal static string SwapColonAndSlash(string scriptName)
    {
        var result = new char[scriptName.Length];

        for (var i = 0; i < scriptName.Length; i++)
        {
            if (scriptName[i] == ':')
            {
                result[i] = '/';
            }
            else if (scriptName[i] == '/')
            {
                result[i] = ':';
            }
            else
            {
                result[i] = scriptName[i];
            }
        }

        return new string(result);
    }
}
