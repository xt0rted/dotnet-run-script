using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

using RunScript;

var workingDirectory = Environment.CurrentDirectory;

Environment.SetEnvironmentVariable("INIT_CWD", workingDirectory);

Project? project;
Dictionary<string, string?> scripts;
try
{
    (project, scripts, workingDirectory) = await new ProjectLoader().Load(workingDirectory);
}
catch (Exception ex)
{
    logError(ex.Message);

    return 1;
}

var rootCommand = new RootCommand();
rootCommand.TreatUnmatchedTokensAsErrors = false;
rootCommand.AddGlobalOption(GlobalOptions.IfPresent);
rootCommand.AddGlobalOption(GlobalOptions.ScriptShell);
rootCommand.AddGlobalOption(GlobalOptions.Verbose);

foreach (var (name, script) in scripts.OrderBy(s => s.Key))
{
    var runScript = new RunScriptCommand(
        name,
        script,
        project.ScriptShell,
        scripts,
        workingDirectory);

    rootCommand.AddCommand(runScript);
}

if (!scripts.ContainsKey("env"))
{
    rootCommand.AddCommand(new EnvironmentCommand());
}

var parser = new CommandLineBuilder(rootCommand)
    .UseVersionOption()
    .UseHelp()
    .UseEnvironmentVariableDirective()
    .UseParseDirective()
    .UseSuggestDirective()
    .RegisterWithDotnetSuggest()
    .RunScriptsIfPrsent()
    .UseParseErrorReporting()
    .UseExceptionHandler((ex, ctx) =>
    {
        if (ctx.ParseResult.HasOption(GlobalOptions.Verbose))
        {
            logError(ex.ToString());
        }
        else
        {
            logError(ex.Message);
        }
    })
    .CancelOnProcessTermination()
    .EnableLegacyDoubleDashBehavior()
    .Build();

var parseResult = parser.Parse(args);

return await parseResult.InvokeAsync();

static void logError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
}
