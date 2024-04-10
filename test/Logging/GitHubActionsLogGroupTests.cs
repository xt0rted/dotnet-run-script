namespace RunScript.Logging;

using System.CommandLine.IO;
using System.CommandLine.Rendering;

[Trait("category", "unit")]
public class GitHubActionsLogGroupTests
{
    [Fact]
    public async Task Should_escape_group_name()
    {
        // Given
        var console = new TestConsole();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(console, consoleFormatProvider, verbose: false);

        // When
        createGroup("plain");
        createGroup("percent: %25");
        createGroup("carriage return: \r");
        createGroup("line feed: \n");
        createGroup("everything: %\r\n");

        // Then
        await Verify(console);

        void createGroup(string groupName)
        {
            using var _ = new GitHubActionsLogGroup(consoleWriter, groupName);
        }
    }
}
