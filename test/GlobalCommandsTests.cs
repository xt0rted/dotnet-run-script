namespace RunScript;

using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Threading.Tasks;

[Trait("category", "unit")]
public class GlobalCommandsTests
{
    [Fact]
    public async Task Should_log_all_available_scripts()
    {
        // Given
        var output = new StringWriter();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(output, consoleFormatProvider, verbose: true);

        var scripts = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            { "clean", "echo clean" },
            { "prebuild", "echo prebuild" },
            { "build", "echo build" },
            { "test", "echo test" },
            { "posttest", "echo posttest" },
            { "prepack", "echo pack" },
            { "pack", "echo pack" },
            { "postpack", "echo pack" },
        };

        // When
        GlobalCommands.PrintAvailableScripts(consoleWriter, scripts);

        // Then
        await Verify(output);
    }

    [Fact]
    public async Task Should_log_all_available_environment_variables()
    {
        // Given
        var output = new StringWriter();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(output, consoleFormatProvider, verbose: true);

        var environment = new TestEnvironment("/test/path", isWindows: true);

        // These are reversed to verify they come back sorted
        environment.SetEnvironmentVariable("value4", "value 4");
        environment.SetEnvironmentVariable("value3", "value 3");
        environment.SetEnvironmentVariable("value2", "value 2");
        environment.SetEnvironmentVariable("value1", "value 1");

        // When
        GlobalCommands.PrintEnvironmentVariables(consoleWriter, environment);

        // Then
        await Verify(output);
    }
}
