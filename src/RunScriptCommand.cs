namespace RunScript;

using System.CommandLine.Invocation;

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
            _workingDirectory);

        builder.SetUpEnvironment(scriptShell);

        if (scripts.Length == 0)
        {
            GlobalCommands.PrintAvailableScripts(writer, project.Scripts!);

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

            var scriptRunner = builder.CreateGroupRunner(context.GetCancellationToken());

            var result = await scriptRunner.RunAsync(
                scriptName,
                scriptArgs);

            runResults.Add((scriptName, result));

            if (result != 0)
            {
                break;
            }
        }

        return RunResults(writer, runResults);
    }

    private static int RunResults(IConsoleWriter writer, List<(string scriptName, int exitCode)> results)
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
}
