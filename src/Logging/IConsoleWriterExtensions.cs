namespace RunScript.Logging;

internal static class IConsoleWriterExtensions
{
    public static IDisposable Group(this IConsoleWriter writer, IEnvironment environment, string name)
    {
        var isRunningOnActions = string.Equals(
            environment.GetEnvironmentVariable(EnvironmentVariables.GitHubActions),
            "true",
            StringComparison.OrdinalIgnoreCase);

        var isChildProcess = string.Equals(
            environment.GetEnvironmentVariable(EnvironmentVariables.RunScriptChildProcess),
            "true",
            StringComparison.OrdinalIgnoreCase);

        return isRunningOnActions && !isChildProcess
            ? new GitHubActionsLogGroup(writer, name)
            : new SilentLogGroup();
    }
}
