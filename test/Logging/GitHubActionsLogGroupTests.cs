namespace RunScript.Logging;

using System.CommandLine.Rendering;

[Trait("category", "unit")]
public class GitHubActionsLogGroupTests
{
    [Fact]
    public async Task Should_escape_group_name()
    {
        // Given
        var output = new StringWriter();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(output, consoleFormatProvider, verbose: false);

        // When
        createGroup("plain");
        createGroup("percent: %25");
        createGroup("carriage return: \r");
        createGroup("line feed: \n");
        createGroup("everything: %\r\n");

        // Then
        await Verify(output);

        void createGroup(string groupName)
        {
            using var _ = new GitHubActionsLogGroup(consoleWriter, groupName);
        }
    }
}
