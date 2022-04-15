namespace RunScript;

using System.Collections;

internal class TestEnvironment : IEnvironment
{
    private readonly Dictionary<string, string?> _variables = new(StringComparer.OrdinalIgnoreCase);

    public TestEnvironment(bool isWindows)
    {
        CurrentDirectory = AttributeReader.GetProjectDirectory(GetType().Assembly);
        IsWindows = isWindows;
    }

    public string CurrentDirectory { get; }

    public bool IsWindows { get; }

    public string? GetEnvironmentVariable(string variable)
        => _variables.TryGetValue(variable, out var value) ? value : null;

    public IDictionary GetEnvironmentVariables()
        => _variables;

    public void SetEnvironmentVariable(string variable, string? value)
        => _variables[variable] = value;
}
