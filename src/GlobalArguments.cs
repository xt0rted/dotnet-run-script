namespace RunScript;

public static class GlobalArguments
{
    public static readonly Argument<string[]> Scripts = new("scripts")
    {
        Arity = ArgumentArity.ZeroOrMore,
        Description = "One or more scripts to run",
        DefaultValueFactory = _ => [],
    };
}
