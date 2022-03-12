namespace RunScript;

using System;
using System.CommandLine;
using System.CommandLine.Rendering;

internal class ConsoleWriter
{
    private readonly IConsole _console;
    private readonly bool _verbose;

    public ConsoleWriter(IConsole console, bool verbose)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _verbose = verbose;
    }

    private void WriteLine(AnsiControlCode textColor, bool verbose, string? message = null, params object?[] args)
    {
        if (!_verbose && verbose)
        {
            return;
        }

        if (message is not null)
        {
            _console.Out.Write(textColor.EscapeSequence);

            if (args is not null && args.Length > 0)
            {
                _console.Out.Write(string.Format(message, args));
            }
            else
            {
                _console.Out.Write(message);
            }

            _console.Out.Write(Ansi.Color.Foreground.Default.EscapeSequence);
        }

        _console.Out.Write(Environment.NewLine);
    }
    public void AlertAboutVerbose() =>
        VerboseLine("Verbose mode is on. This will print more information.");

    public void BlankLine() =>
        WriteLine(Ansi.Color.Foreground.LightGray, verbose: false, null);
    public void BlankVerboseLine() =>
        WriteLine(Ansi.Color.Foreground.LightGray, verbose: true, null);

    public void Line(string? message, params object?[] args) =>
        WriteLine(Ansi.Color.Foreground.LightGray, verbose: false, message, args);
    public void VerboseLine(string? message = null, params object?[] args) =>
        WriteLine(Ansi.Color.Foreground.LightGray, verbose: true, "> " + message, args);

    //public void SecondaryLine(string? message, params object?[] args) =>
    //    WriteLine(Ansi.Color.Foreground.DarkGray, verbose: false, message, args);
    //public void VerboseSecondaryLine(string? message, params object?[] args) =>
    //    WriteLine(Ansi.Color.Foreground.DarkGray, verbose: true, "> " + message, args);

    internal void Information(string? message, params object?[] args) =>
        WriteLine(Ansi.Color.Foreground.Cyan, verbose: false, message, args);
    //public void VerboseInformation(string? message, params object?[] args) =>
    //    WriteLine(Ansi.Color.Foreground.Cyan, verbose: true, "> " + message, args);

    public void Banner(params string?[] messages)
    {
        if (messages == null) return;

        BlankLine();

        foreach (var message in messages)
        {
            Information("> {0}", message);
        }

        BlankLine();
    }

    //public void Error(string? message, params object?[] args) =>
    //    WriteLine(Ansi.Color.Foreground.Red, verbose: false, message, args);
}
