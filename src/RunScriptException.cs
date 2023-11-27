namespace RunScript;

public class RunScriptException : Exception
{
    public RunScriptException()
    {
    }

    public RunScriptException(string? message)
        : base(message)
    {
    }

    public RunScriptException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
