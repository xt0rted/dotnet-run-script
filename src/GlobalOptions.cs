namespace RunScript;

public static class GlobalOptions
{
    public static readonly Option<bool> IfPresent = new("--if-present")
    {
        Description = "Don't exit with an error code if the script isn't found",
    };

    public static readonly Option<string> ScriptShell = new("--script-shell")
    {
        Description = "The shell to use when running scripts (cmd, pwsh, sh, etc.)",
        HelpName = "shell",
    };

    public static readonly Option<bool> Verbose = new("--verbose", "-v")
    {
        Description = "Enable verbose output",
    };
}
