namespace RunScript;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

public class EnvironmentCommand : Command, ICommandHandler
{
    private readonly IFormatProvider _consoleFormatProvider;

    public EnvironmentCommand(IFormatProvider consoleFormatProvider)
        : base("env", "List available environment variables")
    {
        _consoleFormatProvider = consoleFormatProvider ?? throw new ArgumentNullException(nameof(consoleFormatProvider));

        Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var writer = new ConsoleWriter(context.Console, _consoleFormatProvider, context.ParseResult.GetValueForOption(GlobalOptions.Verbose));

        writer.VerboseBanner();
        writer.Banner(Name);

        foreach (var (key, value) in environmentVariables().OrderBy(v => v.Key, StringComparer.InvariantCulture))
        {
            writer.Line("{0}={1}", key, value);
        }

        return Task.FromResult(0);

        static IEnumerable<KeyValuePair<string, string>> environmentVariables()
        {
            var variables = Environment.GetEnvironmentVariables();
            foreach (var key in variables.Keys)
            {
                yield return new KeyValuePair<string, string>((string)key!, (string)variables[key!]!);
            }
        }
    }
}
