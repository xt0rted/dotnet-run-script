#pragma warning disable RCS0054

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

using RunScript;

var environment = new EnvironmentWrapper();
var workingDirectory = environment.CurrentDirectory;

environment.SetEnvironmentVariable("INIT_CWD", workingDirectory);

Project? project;
try
{
    (project, workingDirectory) = await new ProjectLoader().LoadAsync(workingDirectory);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.Message);
    Console.ResetColor();

    return 1;
}

var consoleFormatProvider = ConsoleHelpers.FormatInfo(environment);
var rootCommand = new RootCommand();

rootCommand.TreatUnmatchedTokensAsErrors = false;

rootCommand.AddGlobalOption(GlobalOptions.IfPresent);
rootCommand.AddGlobalOption(GlobalOptions.ScriptShell);
rootCommand.AddGlobalOption(GlobalOptions.Verbose);

foreach (var (name, script) in project.Scripts!.OrderBy(s => s.Key))
{
    var runScript = new RunScriptCommand(
        name,
        script,
        project,
        workingDirectory,
        environment,
        consoleFormatProvider);

    rootCommand.AddCommand(runScript);
}

if (!project.Scripts!.ContainsKey("env"))
{
    rootCommand.AddCommand(
        new EnvironmentCommand(
            environment,
            consoleFormatProvider));
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
        var verbose = ctx.ParseResult.HasOption(GlobalOptions.Verbose);
        var writer = new ConsoleWriter(ctx.Console, consoleFormatProvider, verbose);

        if (verbose)
        {
            writer.Error(ex.ToString());
        }
        else
        {
            writer.Error(ex.Message);
        }
    })
    .CancelOnProcessTermination()
    .EnableLegacyDoubleDashBehavior()
    .Build();

var parseResult = parser.Parse(args);

return await parseResult.InvokeAsync();
