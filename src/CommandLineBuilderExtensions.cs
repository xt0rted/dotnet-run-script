namespace RunScript;

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

internal static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder RunScriptsIfPrsent(this CommandLineBuilder builder)
    {
        builder.AddMiddleware(
            async (ctx, next) =>
            {
                // If the command doesn't exist and --if-present is specified then gracefully exit
                if (ctx.ParseResult.Errors.Count > 0 &&
                    ctx.ParseResult.CommandResult.Command.Handler is null &&
                    ctx.ParseResult.HasOption(GlobalOptions.IfPresent))
                {
                    return;
                }

                await next(ctx);
            },
            MiddlewareOrder.ErrorReporting - 1);

        return builder;
    }
}
