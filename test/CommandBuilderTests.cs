namespace RunScript;

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

[Trait("category", "unit")]
[UsesVerify]
public class CommandBuilderTests
{
    private const string DefaultComSpec = @"C:\WINDOWS\system32\cmd.exe";

    [Theory]
    [InlineData("cmd")]
    [InlineData("cmd.exe")]
    [InlineData(@"c:\dir\cmd")]
    [InlineData(@"c:\dir\cmd.exe")]
    public void IsCmdCheck_should_match_cmd_variations(string shell)
    {
        // Given / When / Then
        CommandBuilder.IsCmdCheck.IsMatch(shell).ShouldBeTrue();
    }

    [Theory]
    [InlineData("pwsh")]
    [InlineData("sh")]
    public void IsCmdCheck_should_not_match_non_cmd_variations(string shell)
    {
        // Given / When / Then
        CommandBuilder.IsCmdCheck.IsMatch(shell).ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetUpEnvironment_should_default_to_comspec_env_var_only_on_windows(bool isWindows)
    {
        // Given
        var verifySettings = new VerifySettings();
        verifySettings.UseParameters(isWindows);

        var builder = SetUpTest(isWindows);

        // When
        builder.SetUpEnvironment(scriptShellOverride: null);

        // Then
        await Verify(builder.ProcessContext, verifySettings);
    }

    [Fact]
    public async Task SetUpEnvironment_should_fall_back_to_cmd_on_windows()
    {
        // Given
        var builder = SetUpTest(isWindows: true, comSpec: null);

        // When
        builder.SetUpEnvironment(scriptShellOverride: null);

        // Then
        await Verify(builder.ProcessContext);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetUpEnvironment_should_use_custom_shell(bool isWindows)
    {
        // Given
        var verifySettings = new VerifySettings();
        verifySettings.UseParameters(isWindows);

        var builder = SetUpTest(isWindows);

        // When
        builder.SetUpEnvironment(scriptShellOverride: "pwsh");

        // Then
        await Verify(builder.ProcessContext, verifySettings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateGroupRunner_should_create_a_runner(bool isWindows)
    {
        // Given
        var builder = SetUpTest(isWindows);
        builder.SetUpEnvironment(scriptShellOverride: null);

        // When
        var groupRunner = builder.CreateGroupRunner(default);

        // Then
        groupRunner.ShouldNotBeNull();
        groupRunner.ShouldBeOfType<CommandGroupRunner>();
    }

    private static CommandBuilder SetUpTest(bool isWindows, string? comSpec = DefaultComSpec)
    {
        var console = new TestConsole();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(console, consoleFormatProvider, verbose: true);

        var project = new Project
        {
            Scripts = new Dictionary<string, string?>(),
        };

        var environment = new TestEnvironment(
            "/test/path",
            isWindows);

        if (isWindows)
        {
            environment.SetEnvironmentVariable("COMSPEC", comSpec);
        }

        return new CommandBuilder(
            consoleWriter,
            environment,
            project,
            "/test/path",
            captureOutput: true);
    }
}
