namespace RunScript;

using System.Collections;

internal interface IEnvironment
{
    string CurrentDirectory { get; }

    bool IsWindows { get; }

    string? GetEnvironmentVariable(string variable);

    IDictionary GetEnvironmentVariables();

    void SetEnvironmentVariable(string variable, string? value);
}
