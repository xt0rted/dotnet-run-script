namespace RunScript;

using System.CommandLine.Rendering;

internal static class ConsoleHelpers
{
    public static ConsoleFormatInfo FormatInfo(IEnvironment environment)
        => ConsoleFormatInfo.ReadOnly(FormatInfoBuilder(environment));

    private static ConsoleFormatInfo FormatInfoBuilder(IEnvironment environment)
    {
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = ConsoleFormatInfo.CurrentInfo.SupportsAnsiCodes,
        };

        if (environment.GetEnvironmentVariable("NO_COLOR") is not null)
        {
            consoleFormatProvider.SupportsAnsiCodes = false;

            return consoleFormatProvider;
        }

        var envVar = environment.GetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION");

        if (envVar is not null)
        {
            consoleFormatProvider.SupportsAnsiCodes = envVar == "1" || envVar.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        return consoleFormatProvider;
    }
}
