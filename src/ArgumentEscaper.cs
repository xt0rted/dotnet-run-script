namespace RunScript;

/// <summary>
/// Copy of the internal sdk class to escape command arguments updated to use <c>ReadOnlySpan</c>.
/// </summary>
/// <remarks>
/// <seealso href="https://github.com/dotnet/sdk/blob/09b31215867d1ffe4955fd5b7cd91eb552d3632c/src/Cli/Microsoft.DotNet.Cli.Utils/ArgumentEscaper.cs"/>
/// </remarks>
internal static class ArgumentEscaper
{
    private const char Backslash = '\\';
    private const char Caret = '^';
    private const char NewLine = '\n';
    private const char Quote = '"';
    private const char Space = ' ';
    private const char Tab = '\t';

    /// <summary>
    /// Undo the processing which took place to create string[] args in Main, so that the next process will receive the same string[] args.
    /// </summary>
    /// <remarks>
    /// See here for more info: <seealso href="https://docs.microsoft.com/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way"/>.
    /// </remarks>
    /// <param name="args">List of arguments to escape.</param>
    /// <returns>An escaped string of the supplied arguments.</returns>
    public static string EscapeAndConcatenateArgArrayForProcessStart(IReadOnlyList<string>? args)
    {
        if (args is null)
        {
            return "";
        }

        return EscapeArgArray(args);
    }

    /// <summary>
    /// Undo the processing which took place to create string[] args in Main, so that the next process will receive the same string[] args.
    /// </summary>
    /// <remarks>
    /// See here for more info: <seealso href="https://docs.microsoft.com/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way"/>.
    /// </remarks>
    /// <param name="args">List of arguments to escape.</param>
    /// <returns>A <c>cmd.exe</c> safe escaped string of the supplied arguments.</returns>
    public static string EscapeAndConcatenateArgArrayForCmdProcessStart(IReadOnlyList<string>? args)
    {
        if (args is null)
        {
            return "";
        }

        return EscapeArgArrayForCmd(args);
    }

    /// <summary>
    /// Undo the processing which took place to create string[] args in Main, so that the next process will receive the same string[] args.
    /// </summary>
    /// <remarks>
    /// See here for more info: <seealso href="https://docs.microsoft.com/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way"/>.
    /// </remarks>
    /// <param name="arguments">List of arguments to escape.</param>
    /// <returns>An escaped string of the supplied arguments.</returns>
    private static string EscapeArgArray(IReadOnlyList<string> arguments)
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);

        for (var i = 0; i < arguments.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(Space);
            }

            EscapeSingleArg(ref sb, arguments[i]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// This prefixes every character with the '^' character to force cmd to interpret the argument string literally.
    /// </summary>
    /// <remarks>
    /// See here for more info: <seealso href="https://docs.microsoft.com/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way"/>.
    /// </remarks>
    /// <param name="arguments">List of arguments to escape.</param>
    /// <returns>A <c>cmd.exe</c> safe escaped string of the supplied arguments.</returns>
    private static string EscapeArgArrayForCmd(IReadOnlyList<string> arguments)
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);

        for (var i = 0; i < arguments.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(Space);
            }

            EscapeArgForCmd(ref sb, arguments[i]);
        }

        return sb.ToString();
    }

    private static void EscapeSingleArg(ref ValueStringBuilder sb, string arg)
    {
        var length = arg.Length;
        var needsQuotes = length == 0 || ArgumentContainsWhitespace(arg);
        var isQuoted = needsQuotes || IsSurroundedWithQuotes(arg);

        if (needsQuotes)
        {
            sb.Append(Quote);
        }

        for (var i = 0; i < length; ++i)
        {
            var backslashCount = 0;

            // Consume All Backslashes
            while (i < arg.Length && arg[i] == Backslash)
            {
                backslashCount++;
                i++;
            }

            // Escape any backslashes at the end of the arg
            // when the argument is also quoted.
            // This ensures the outside quote is interpreted as
            // an argument delimiter
            if (i == arg.Length && isQuoted)
            {
                sb.Append(Backslash, 2 * backslashCount);
            }

            // At then end of the arg, which isn't quoted,
            // just add the backslashes, no need to escape
            else if (i == arg.Length)
            {
                sb.Append(Backslash, backslashCount);
            }

            // Escape any preceding backslashes and the quote
            else if (arg[i] == Quote)
            {
                sb.Append(Backslash, (2 * backslashCount) + 1);
                sb.Append(Quote);
            }

            // Output any consumed backslashes and the character
            else
            {
                sb.Append(Backslash, backslashCount);
                sb.Append(arg[i]);
            }
        }

        if (needsQuotes)
        {
            sb.Append(Quote);
        }
    }

    /// <summary>
    /// <para>Prepare as single argument to roundtrip properly through cmd.</para>
    /// <para>This prefixes every character with the <c>^</c> character to force cmd to interpret the argument string literally.</para>
    /// </summary>
    /// <remarks>
    /// See here for more info: <seealso href="https://docs.microsoft.com/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way"/>.
    /// </remarks>
    /// <param name="sb">The <seealso cref="ValueStringBuilder"/> to append to.</param>
    /// <param name="argument">The argument to escape.</param>
    private static void EscapeArgForCmd(ref ValueStringBuilder sb, string argument)
    {
        var quoted = ArgumentContainsWhitespace(argument);

        if (quoted)
        {
            sb.Append(Caret);
            sb.Append(Quote);
        }

        // Prepend every character with ^
        // This is harmless when passing through cmd
        // and ensures cmd metacharacters are not interpreted
        // as such
        foreach (var character in argument)
        {
            sb.Append(Caret);
            sb.Append(character);
        }

        if (quoted)
        {
            sb.Append(Caret);
            sb.Append(Quote);
        }
    }

    private static bool IsSurroundedWithQuotes(ReadOnlySpan<char> argument)
        => argument[0] == Quote &&
            argument[^1] == Quote;

    private static bool ArgumentContainsWhitespace(ReadOnlySpan<char> argument)
        => argument.Contains(Space) ||
            argument.Contains(Tab) ||
            argument.Contains(NewLine);
}
