namespace RunScript;

/// <summary>
/// Execute scripts in a separate process.
/// </summary>
internal interface ICommandRunner
{
    /// <summary>
    /// Run the specified command.
    /// </summary>
    /// <param name="name">The name of the command to run.</param>
    /// <param name="cmd">The command to run.</param>
    /// <param name="args">Any arguments to pass to the command.</param>
    /// <returns>The exit code of the command.</returns>
    Task<int> RunAsync(string name, string cmd, string[]? args);
}
