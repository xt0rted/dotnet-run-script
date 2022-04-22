namespace RunScript;

using System.Collections;
using System.Runtime.InteropServices;

internal class EnvironmentWrapper : IEnvironment
{
    public string CurrentDirectory => Environment.CurrentDirectory;

    public bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public string? GetEnvironmentVariable(string variable)
        => Environment.GetEnvironmentVariable(variable);

    public IDictionary GetEnvironmentVariables()
        => Environment.GetEnvironmentVariables();

    public void SetEnvironmentVariable(string variable, string? value)
        => Environment.SetEnvironmentVariable(variable, value);
}
