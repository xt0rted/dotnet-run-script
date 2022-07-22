namespace RunScript.Integration;

using System;
using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

public static class CommandBuilderTests
{
    static CommandBuilderTests()
    {
        VerifierSettings.AddExtraSettings(settings => settings.Converters.Add(new ConsoleConverter()));
    }

    // Passing `bash` works locally, but not on CI due to no WSL so the next best thing is using git bash
    [Trait("category", "integration")]
    [UsesVerify]
    public class WindowsPlatform
    {
        [WindowsTheory]
        [InlineData(null)]
        [InlineData("cmd.exe")]
        [InlineData("pwsh")]
        [InlineData(@"C:\Program Files\Git\bin\bash.exe")]
        public async Task Should_execute_single_script_in_shell(string shellOverride)
        {
            await CommandBuilderTests.Should_execute_single_script_in_shell(
                isWindows: true,
                shellOverride);
        }
    }

    [Trait("category", "integration")]
    [UsesVerify]
    public class UnixPlatforms
    {
        [UnixTheory]
        [InlineData(null)]
        [InlineData("pwsh")]
        public async Task Should_execute_single_script_in_shell(string shellOverride)
        {
            await CommandBuilderTests.Should_execute_single_script_in_shell(
                isWindows: false,
                shellOverride);
        }
    }

    private static async Task Should_execute_single_script_in_shell(bool isWindows, string shellOverride)
    {
        var console = new TestConsole();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(console, consoleFormatProvider, verbose: true);

        var environment = new TestEnvironment(isWindows: isWindows);

        var project = new Project
        {
            Scripts = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                { "test", "echo testing" },
            },
        };

        var cb = new CommandBuilder(
            consoleWriter,
            environment,
            project,
            environment.CurrentDirectory,
            // This lets us verify the output
            captureOutput: true);

        cb.SetUpEnvironment(scriptShellOverride: shellOverride);

        using var ct = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        var gr = cb.CreateGroupRunner(ct.Token);

        var result = await gr.RunAsync(
            name: "test",
            scriptArgs: null);

        await Verify(console).UseParameters(ShellName(shellOverride));

        result.ShouldBe(0);
    }

    private static string ShellName(string? shell)
    {
        if (shell is null)
        {
            return "default";
        }

        if (shell.StartsWith(@"c:\", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(shell);
        }

        return shell;
    }
}
