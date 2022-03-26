namespace RunScript;

using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Globalization;

internal class ConsoleWriter : IConsoleWriter
{
    private readonly IConsole _console;
    private readonly IFormatProvider? _consoleFormatProvider;

    private readonly bool _verbose;

    public ConsoleWriter(IConsole console, IFormatProvider consoleFormatProvider, bool verbose)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _consoleFormatProvider = consoleFormatProvider ?? throw new ArgumentNullException(nameof(consoleFormatProvider));
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
            _console.Out.Write(textColor.ToString(null, _consoleFormatProvider));

            if (args?.Length > 0)
            {
                _console.Out.Write(string.Format(CultureInfo.CurrentCulture, message, args));
            }
            else
            {
                _console.Out.Write(message);
            }

            _console.Out.Write(Ansi.Color.Foreground.Default.ToString(null, _consoleFormatProvider));
        }

        _console.Out.Write(Environment.NewLine);
    }

    public void VerboseBanner()
        => LineVerbose("Verbose mode is on. This will print more information.");

    public void BlankLine()
        => WriteLine(Ansi.Color.Foreground.LightGray, verbose: false, null);

    public void BlankLineVerbose()
        => WriteLine(Ansi.Color.Foreground.LightGray, verbose: true, null);

    public void Line(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.LightGray, verbose: false, message, args);

    public void LineVerbose(string? message = null, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.LightGray, verbose: true, "> " + message, args);

    internal void Information(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.Cyan, verbose: false, message, args);

    public void Banner(params string?[] messages)
    {
        if (messages is null) return;

        BlankLine();

        foreach (var message in messages)
        {
            Information("> {0}", message);
        }

        BlankLine();
    }

    public void Error(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.Red, verbose: false, message, args);
}
