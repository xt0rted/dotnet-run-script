namespace RunScript;

/// <summary>
/// Execute a script and any pre &amp; post scripts.
/// </summary>
internal interface ICommandGroupRunner
{
    /// <summary>
    /// Run the script and it's pre &amp; post scripts.
    /// </summary>
    /// <param name="name">The name of the script to run.</param>
    /// <param name="scriptArgs">
    /// Any arguments to pass to the executing script.
    /// Will not be passed to the <c>pre</c> or <c>post</c> scripts.
    /// </param>
    /// <returns><c>0</c> if there were no errors; otherwise the exit code of the failed script.</returns>
    Task<int> RunAsync(string name, string[]? scriptArgs);
}
