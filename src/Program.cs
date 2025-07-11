using RunScript;

var environment = new EnvironmentWrapper();
var consoleFormatProvider = ConsoleHelpers.FormatInfo(environment);
var rootCommand = new RunScriptCommand(
    environment,
    consoleFormatProvider,
    environment.CurrentDirectory)
{
    new DiagramDirective(),
    new EnvironmentVariablesDirective()
};

var parseResult = rootCommand.Parse(args);

return await parseResult.InvokeAsync();
