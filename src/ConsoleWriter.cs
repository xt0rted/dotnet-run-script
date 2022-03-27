namespace RunScript;

using System.CommandLine;
using System.CommandLine.Rendering;
using System.Globalization;

internal class ConsoleWriter : IConsoleWriter
{
    private static readonly object _writeLock = new();

    private readonly IConsole _console;
    private readonly IFormatProvider? _consoleFormatProvider;

    private readonly bool _verbose;

    public ConsoleWriter(IConsole console, IFormatProvider consoleFormatProvider, bool verbose)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _consoleFormatProvider = consoleFormatProvider ?? throw new ArgumentNullException(nameof(consoleFormatProvider));
        _verbose = verbose;
    }

    private void WriteLine(AnsiControlCode modifierOn, AnsiControlCode modifierOff, bool verbose, string? message = null, params object?[] args)
    {
        if (!_verbose && verbose)
        {
            return;
        }

        lock (_writeLock)
        {
            if (message is not null)
            {
                _console.Out.Write(modifierOn.ToString(null, _consoleFormatProvider));

                if (args?.Length > 0)
                {
                    _console.Out.Write(string.Format(CultureInfo.CurrentCulture, message, args));
                }
                else
                {
                    _console.Out.Write(message);
                }

                _console.Out.Write(modifierOff.ToString(null, _consoleFormatProvider));
            }

            _console.Out.Write(Environment.NewLine);
        }
    }

    public void VerboseBanner()
        => LineVerbose("Verbose mode is on. This will print more information.");

    public void BlankLine()
        => WriteLine(Ansi.Color.Foreground.LightGray, Ansi.Color.Foreground.Default, verbose: false, null);

    public void BlankLineVerbose()
        => WriteLine(Ansi.Color.Foreground.LightGray, Ansi.Color.Foreground.Default, verbose: true, null);

    public void Line(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.LightGray, Ansi.Color.Foreground.Default, verbose: false, message, args);

    public void LineVerbose(string? message = null, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.LightGray, Ansi.Color.Foreground.Default, verbose: true, "> " + message, args);

    public void SecondaryLine(string? message, params object?[] args)
        => WriteLine(TextDim, Ansi.Text.BoldOff, verbose: false, message, args);

    public void VerboseSecondaryLine(string? message, params object?[] args)
        => WriteLine(TextDim, Ansi.Text.BoldOff, verbose: true, "> " + message, args);

    internal void Information(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.Cyan, Ansi.Color.Foreground.Default, verbose: false, message, args);

    public void Banner(params string?[] messages)
    {
        if (messages is null) return;

        lock (_writeLock)
        {
            BlankLine();

            foreach (var message in messages)
            {
                Information("> {0}", message);
            }

            BlankLine();
        }
    }

    public void Error(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.Red, Ansi.Color.Foreground.Default, verbose: false, message, args);

    public string ColorText(ConsoleColor color, int value)
        => ColorText(color, value.ToString(CultureInfo.CurrentCulture));

    public string ColorText(ConsoleColor color, string value)
    {
        var colorControlCode = color switch
        {
            ConsoleColor.DarkBlue => Ansi.Color.Foreground.Blue,
            ConsoleColor.DarkGreen => Ansi.Color.Foreground.Green,
            ConsoleColor.DarkCyan => Ansi.Color.Foreground.Cyan,
            ConsoleColor.DarkRed => Ansi.Color.Foreground.Red,
            ConsoleColor.DarkMagenta => Ansi.Color.Foreground.Magenta,
            ConsoleColor.DarkYellow => Ansi.Color.Foreground.Yellow,
            ConsoleColor.Gray => Ansi.Color.Foreground.LightGray,
            ConsoleColor.DarkGray => Ansi.Color.Foreground.DarkGray,
            ConsoleColor.Blue => Ansi.Color.Foreground.LightBlue,
            ConsoleColor.Green => Ansi.Color.Foreground.LightGreen,
            ConsoleColor.Cyan => Ansi.Color.Foreground.LightCyan,
            ConsoleColor.Red => Ansi.Color.Foreground.LightRed,
            ConsoleColor.Magenta => Ansi.Color.Foreground.LightMagenta,
            ConsoleColor.Yellow => Ansi.Color.Foreground.LightYellow,
            ConsoleColor.White => Ansi.Color.Foreground.White,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null),
        };

        return string.Concat(
            colorControlCode.ToString(null, _consoleFormatProvider),
            value,
            Ansi.Color.Foreground.Default.ToString(null, _consoleFormatProvider));
    }

    private static AnsiControlCode TextDim { get; } = $"{Ansi.Esc}[2m";
}
