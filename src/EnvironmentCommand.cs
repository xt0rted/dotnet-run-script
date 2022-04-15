namespace RunScript;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

internal class EnvironmentCommand : Command, ICommandHandler
{
    private readonly IEnvironment _environment;
    private readonly IFormatProvider _consoleFormatProvider;

    public EnvironmentCommand(
        IEnvironment environment,
        IFormatProvider consoleFormatProvider)
        : base("env", "List available environment variables")
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _consoleFormatProvider = consoleFormatProvider ?? throw new ArgumentNullException(nameof(consoleFormatProvider));

        Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var writer = new ConsoleWriter(context.Console, _consoleFormatProvider, context.ParseResult.GetValueForOption(GlobalOptions.Verbose));

        writer.VerboseBanner();
        writer.Banner(Name);

        foreach (var (key, value) in environmentVariables(_environment).OrderBy(v => v.key, StringComparer.InvariantCulture))
        {
            writer.Line("{0}={1}", key, value);
        }

        return Task.FromResult(0);

        static IEnumerable<(string key, string value)> environmentVariables(IEnvironment environment)
        {
            var variables = environment.GetEnvironmentVariables();

            foreach (var key in variables.Keys)
            {
                yield return new ((string)key!, (string)variables[key!]!);
            }
        }
    }
}
