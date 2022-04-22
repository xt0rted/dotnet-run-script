namespace RunScript;

public class ConsoleHelpersTests
{
    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("TrUe")]
    [InlineData("TRUE")]
    [InlineData("0")]
    [InlineData("false")]
    [InlineData("FaLsE")]
    [InlineData("FALSE")]
    [InlineData("no")]
    [InlineData("anything else")]
    public void FormatInfoBuilder_should_support_NO_COLOR_env_var(string value)
    {
        // Given
        var environment = new TestEnvironment();

        environment.SetEnvironmentVariable("NO_COLOR", value);

        // When
        var result = ConsoleHelpers.FormatInfo(environment);

        // Then
        result.SupportsAnsiCodes.ShouldBeFalse();
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData("TrUe", true)]
    [InlineData("TRUE", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("FaLsE", false)]
    [InlineData("FALSE", false)]
    [InlineData("no", false)]
    [InlineData("anything else", false)]
    public void FormatInfoBuilder_should_support_DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION_env_var(string value, bool expected)
    {
        // Given
        var environment = new TestEnvironment();

        environment.SetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION", value);

        // When
        var result = ConsoleHelpers.FormatInfo(environment);

        // Then
        result.SupportsAnsiCodes.ShouldBe(expected);
    }
}
