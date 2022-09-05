namespace RunScript.Logging;

internal sealed class GitHubActionsLogGroup : IDisposable
{
    private readonly IConsoleWriter _writer;

    public GitHubActionsLogGroup(IConsoleWriter writer, string name)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        _writer.Raw("::group::" + EscapeData("dotnet r " + name) + Environment.NewLine);
    }

    public void Dispose()
        => _writer.Raw("::endgroup::" + Environment.NewLine);

    private static string EscapeData(string value)
        => value
            .Replace("%", "%25", StringComparison.OrdinalIgnoreCase)
            .Replace("\r", "%0D", StringComparison.OrdinalIgnoreCase)
            .Replace("\n", "%0A", StringComparison.OrdinalIgnoreCase);
}
