namespace RunScript;

using System.Collections;
using System.Runtime.InteropServices;

internal class TestEnvironment : IEnvironment
{
    private readonly Dictionary<string, string?> _variables = new(StringComparer.OrdinalIgnoreCase);

    public TestEnvironment(string? currentDirectory = null, bool? isWindows = null)
    {
        CurrentDirectory = currentDirectory ?? AttributeReader.GetProjectDirectory(GetType().Assembly);
        IsWindows = isWindows ?? RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
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
