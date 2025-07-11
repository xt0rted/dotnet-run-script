namespace RunScript;

using System;
using System.CommandLine.Rendering;
using System.Globalization;

internal class ConsoleWriter : IConsoleWriter
{
    private readonly TextWriter _output;
    private readonly IFormatProvider _consoleFormatProvider;
    private readonly bool _verbose;

    public ConsoleWriter(TextWriter output, IFormatProvider consoleFormatProvider, bool verbose)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(consoleFormatProvider);

        _output = output;
        _consoleFormatProvider = consoleFormatProvider;
        _verbose = verbose;
    }

    private static AnsiControlCode TextDim { get; } = $"{Ansi.Esc}[2m";

    private void WriteLine(AnsiControlCode modifierOn, AnsiControlCode modifierOff, string? message, params object?[] args)
    {
        if (message is not null)
        {
            _output.Write(modifierOn.ToString(null, _consoleFormatProvider));

            if (args?.Length > 0)
            {
                _output.Write(string.Format(CultureInfo.CurrentCulture, message, args));
            }
            else
            {
                _output.Write(message);
            }

            _output.Write(modifierOff.ToString(null, _consoleFormatProvider));
            _output.Write(Environment.NewLine);
        }
    }

    public void Raw(string? message)
        => _output.Write(message);

    public void VerboseBanner()
        => LineVerbose("Verbose mode is on. This will print more information.");

    public void BlankLine()
        => _output.Write(Environment.NewLine);

    public void BlankLineVerbose()
    {
        if (_verbose)
        {
            _output.Write(Environment.NewLine);
        }
    }

    public void Line(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.LightGray, Ansi.Color.Foreground.Default, message, args);

    public void LineVerbose(string? message = null, params object?[] args)
    {
        if (_verbose)
        {
            WriteLine(Ansi.Color.Foreground.LightGray, Ansi.Color.Foreground.Default, "> " + message, args);
        }
    }

    public void SecondaryLine(string? message, params object?[] args)
        => WriteLine(TextDim, Ansi.Text.BoldOff, message, args);

    public void VerboseSecondaryLine(string? message, params object?[] args)
    {
        if (_verbose)
        {
            WriteLine(TextDim, Ansi.Text.BoldOff, "> " + message, args);
        }
    }

    internal void Information(string? message, params object?[] args)
        => WriteLine(Ansi.Color.Foreground.Cyan, Ansi.Color.Foreground.Default, message, args);

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
        => WriteLine(Ansi.Color.Foreground.Red, Ansi.Color.Foreground.Default, message, args);

    public string? ColorText(ConsoleColor color, int value)
        => ColorText(color, value.ToString(CultureInfo.CurrentCulture));

    public string? ColorText(ConsoleColor color, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

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
}
