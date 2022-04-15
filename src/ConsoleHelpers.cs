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

        if (consoleFormatProvider.SupportsAnsiCodes)
        {
            consoleFormatProvider.SupportsAnsiCodes = environment.GetEnvironmentVariable("NO_COLOR") is null;
        }
        else
        {
            var envVar = environment.GetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION");

            consoleFormatProvider.SupportsAnsiCodes = envVar is not null && (envVar == "1" || envVar.Equals("true", StringComparison.OrdinalIgnoreCase));
        }

        return consoleFormatProvider;
    }
}
