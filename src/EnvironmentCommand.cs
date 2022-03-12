namespace RunScript;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

public class EnvironmentCommand : Command, ICommandHandler
{
    public EnvironmentCommand()
        : base("env", "List available environment variables")
    {
        Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var console = new ConsoleWriter(context.Console, context.ParseResult.GetValueForOption(GlobalOptions.Verbose));
        console.AlertAboutVerbose();

        console.Banner(Name);

        foreach (var (key, value) in environmentVariables().OrderBy(v => v.Key, StringComparer.InvariantCulture))
        {
            console.Line("{0}={1}", key, value);
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
