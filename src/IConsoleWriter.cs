namespace RunScript;

internal interface IConsoleWriter
{
    void VerboseBanner();

    void BlankLine();

    void BlankLineVerbose();

    void Line(string? message, params object?[] args);

    void LineVerbose(string? message = null, params object?[] args);

    void Banner(params string?[] messages);

    void Error(string? message, params object?[] args);
}
