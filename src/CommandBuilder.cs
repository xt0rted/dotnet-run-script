namespace RunScript;

using System.Text.RegularExpressions;

internal partial class CommandBuilder
{
    private readonly IConsoleWriter _writer;
    private readonly IEnvironment _environment;
    private readonly Project _project;
    private readonly string _workingDirectory;
    private readonly bool _captureOutput;

    public CommandBuilder(
        IConsoleWriter writer,
        IEnvironment environment,
        Project project,
        string workingDirectory,
        bool captureOutput)
    {
        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));

        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _workingDirectory = workingDirectory;
        _captureOutput = captureOutput;
    }

    public ProcessContext? ProcessContext { get; private set; }

    // This is the same regex used by npm's run-script library
    [GeneratedRegex("(?:^|\\\\)cmd(?:\\.exe)?$", RegexOptions.IgnoreCase)]
    public static partial Regex IsCmdCheck();

    public void SetUpEnvironment(string? scriptShellOverride)
    {
        var (scriptShell, isCmd) = GetScriptShell(scriptShellOverride ?? _project.ScriptShell);

        ProcessContext = ProcessContext.Create(scriptShell, isCmd, _workingDirectory);

        _writer.LineVerbose("Using shell: {0}", scriptShell);
        _writer.BlankLine();
    }

    public ICommandGroupRunner CreateGroupRunner(CancellationToken cancellationToken)
        => new CommandGroupRunner(
            _writer,
            _environment,
            _project.Scripts!,
            ProcessContext!,
            _captureOutput,
            cancellationToken);

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

        var isCmd = IsCmdCheck().IsMatch(shell);

        return (shell, isCmd);
    }
}
