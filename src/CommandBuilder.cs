namespace RunScript;

using System.Text.RegularExpressions;

internal class CommandBuilder
{
    // This is the same regex used by npm's run-script library
    public static readonly Regex IsCmdCheck = new("(?:^|\\\\)cmd(?:\\.exe)?$", RegexOptions.IgnoreCase);

    private readonly IConsoleWriter _writer;
    private readonly IEnvironment _environment;
    private readonly Project _project;
    private readonly string _workingDirectory;

    public CommandBuilder(
        IConsoleWriter writer,
        IEnvironment environment,
        Project project,
        string workingDirectory)
    {
        if (string.IsNullOrEmpty(workingDirectory)) throw new ArgumentException($"'{nameof(workingDirectory)}' cannot be null or empty.", nameof(workingDirectory));

        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _workingDirectory = workingDirectory;
    }

    public ProcessContext? ProcessContext { get; private set; }

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

        var isCmd = IsCmdCheck.IsMatch(shell);

        return (shell, isCmd);
    }
}
