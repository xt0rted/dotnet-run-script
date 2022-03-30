namespace RunScript;

using System.Collections.Immutable;

internal class CommandGroupRunner : ICommandGroupRunner
{
    private readonly IConsoleWriter _writer;
    private readonly IEnvironment _environment;
    private readonly IDictionary<string, string?> _scripts;
    private readonly ProcessContext _processContext;
    private readonly CancellationToken _cancellationToken;

    public CommandGroupRunner(
        IConsoleWriter writer,
        IEnvironment environment,
        IDictionary<string, string?> scripts,
        ProcessContext processContext,
        CancellationToken cancellationToken)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
        _processContext = processContext ?? throw new ArgumentNullException(nameof(processContext));
        _cancellationToken = cancellationToken;
    }

    public async Task<int> RunAsync(string name, string[]? scriptArgs)
    {
        var scriptNames = ImmutableArray.Create(new[] { "pre" + name, name, "post" + name });

        foreach (var subScript in scriptNames.Where(scriptName => _scripts.ContainsKey(scriptName) || scriptName == "env"))
        {
            // At this point we should have done enough checks to make sure the only not found script is `env`
            if (!_scripts.ContainsKey(subScript))
            {
                GlobalCommands.PrintEnvironmentVariables(_writer, _environment);

                continue;
            }

            ICommandRunner command = new CommandRunner(
                _writer,
                _processContext,
                _cancellationToken);

            var args = subScript == name
                ? scriptArgs
                : null;

            var result = await command.RunAsync(
                subScript,
                _scripts[subScript]!,
                args);

            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }
}
