namespace RunScript;

using System.Runtime.Serialization;

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

    protected RunScriptException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
