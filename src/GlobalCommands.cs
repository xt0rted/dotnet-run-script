namespace RunScript;

internal static class GlobalCommands
{
    /// <summary>
    /// The help command that lists all the scripts availble in the <g>global.json</g>.
    /// </summary>
    /// <param name="writer">The console logger instance to use.</param>
    /// <param name="scripts">The project's scripts.</param>
    public static void PrintAvailableScripts(IConsoleWriter writer, IDictionary<string, string?> scripts)
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
    /// <param name="environment">The environment wrapper to use.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void PrintEnvironmentVariables(IConsoleWriter writer, IEnvironment environment)
    {
        if (environment is null) throw new ArgumentNullException(nameof(environment));

        writer.Banner("env");

        foreach (var (key, value) in environmentVariables(environment).OrderBy(v => v.key, StringComparer.InvariantCulture))
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
}
