namespace RunScript;

using System.Collections.Immutable;

internal class CommandGroupRunner : ICommandGroupRunner
{
    private readonly IConsoleWriter _writer;
    private readonly IEnvironment _environment;
    private readonly ScriptCollection _scripts;
    private readonly ProcessContext _processContext;
    private readonly bool _captureOutput;
    private readonly CancellationToken _cancellationToken;

    public CommandGroupRunner(
        IConsoleWriter writer,
        IEnvironment environment,
        ScriptCollection scripts,
        ProcessContext processContext,
        bool captureOutput,
        CancellationToken cancellationToken)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _scripts = scripts ?? throw new ArgumentNullException(nameof(scripts));
        _processContext = processContext ?? throw new ArgumentNullException(nameof(processContext));
        _captureOutput = captureOutput;
        _cancellationToken = cancellationToken;
    }

    public virtual ICommandRunner BuildCommand()
        => new CommandRunner(
            _writer,
            _processContext,
            _captureOutput,
            _cancellationToken);

    public async Task<int> RunAsync(string name, string[]? scriptArgs)
    {
        var scriptNames = ImmutableArray.Create(new[] { "pre" + name, name, "post" + name });

        foreach (var subScript in scriptNames.Where(scriptName => _scripts.Contains(scriptName) || scriptName == "env"))
        {
            // At this point we should have done enough checks to make sure the only not found script is `env`
            if (!_scripts.Contains(subScript))
            {
                GlobalCommands.PrintEnvironmentVariables(_writer, _environment);

                continue;
            }

            var command = BuildCommand();

            var args = subScript == name
                ? scriptArgs
                : null;

            var result = await command.RunAsync(
                subScript,
                _scripts[subScript].Script,
                args);

            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }
}
