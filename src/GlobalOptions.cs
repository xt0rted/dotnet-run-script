namespace RunScript;

using System.CommandLine;

public static class GlobalOptions
{
    public static readonly Option<bool> IfPresent = new("--if-present", "Don't exit with an error code if the script isn't found");

    public static readonly Option<string> ScriptShell = new("--script-shell", "The shell to use when running scripts (cmd, pwsh, sh, etc.)")
    {
        ArgumentHelpName = "shell",
    };

    public static readonly Option<bool> Verbose = new("--verbose", "Enable verbose output");
}
