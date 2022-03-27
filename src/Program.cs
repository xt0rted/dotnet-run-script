using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

using RunScript;

var environment = new EnvironmentWrapper();
var workingDirectory = environment.CurrentDirectory;

environment.SetEnvironmentVariable("INIT_CWD", workingDirectory);

var consoleFormatProvider = ConsoleHelpers.FormatInfo(environment);
var rootCommand = new RunScriptCommand(
    environment,
    consoleFormatProvider,
    workingDirectory);

var parser = new CommandLineBuilder(rootCommand)
    .UseVersionOption()
    .UseHelp()
    .UseEnvironmentVariableDirective()
    .UseParseDirective()
    .UseSuggestDirective()
    .RegisterWithDotnetSuggest()
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
