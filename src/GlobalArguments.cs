namespace RunScript;

public static class GlobalArguments
{
    public static readonly Argument<string[]> Scripts = new("scripts", "One or more scripts to run")
    {
        Arity = ArgumentArity.ZeroOrMore,
    };
}
