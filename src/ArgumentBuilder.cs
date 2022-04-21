namespace RunScript;

using System.Runtime.CompilerServices;

/// <summary>
/// Copy of the internal sdk class to escape command arguments updated to use <c>ReadOnlySpan</c>.
/// </summary>
/// <remarks>
/// <seealso href="https://github.com/dotnet/sdk/blob/09b31215867d1ffe4955fd5b7cd91eb552d3632c/src/Cli/Microsoft.DotNet.Cli.Utils/ArgumentEscaper.cs"/>
/// </remarks>
internal static class ArgumentBuilder
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
    /// <param name="command">The base command.</param>
    /// <param name="args">List of arguments to escape.</param>
    /// <returns>An escaped string of the <paramref name="command"/> and <paramref name="args"/>.</returns>
    public static string EscapeAndConcatenateCommandAndArgArrayForProcessStart(
        string? command,
        string[]? args)
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);

        sb.Append(command);

        if (args is not null)
        {
            for (var i = 0; i < args.Length; i++)
            {
                sb.Append(Space);

                EscapeSingleArg(ref sb, args[i]);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Undo the processing which took place to create string[] args in Main, so that the next process will receive the same string[] args.
    /// </summary>
    /// <remarks>
    /// See here for more info: <seealso href="https://docs.microsoft.com/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way"/>.
    /// </remarks>
    /// <param name="command">The base command.</param>
    /// <param name="args">List of arguments to escape.</param>
    /// <returns>An escaped string of the <paramref name="command"/> and <paramref name="args"/>.</returns>
    public static string EscapeAndConcatenateCommandAndArgArrayForCmdProcessStart(
        string? command,
        string[]? args)
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);

        EscapeArgForCmd(ref sb, command, quoteValue: false);

        if (args is not null)
        {
            for (var i = 0; i < args.Length; i++)
            {
                sb.Append(Caret);
                sb.Append(Space);

                EscapeArgForCmd(ref sb, args[i], quoteValue: true);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Concatinates the command and arguments without any escaping.
    /// This is meant to be used for display only and not for passing to a new process.
    /// </summary>
    /// <param name="command">The base command.</param>
    /// <param name="args">List of optional arguments.</param>
    /// <returns>A raw concatination of the <paramref name="command"/> and <paramref name="args"/>.</returns>
    public static string ConcatinateCommandAndArgArrayForDisplay(
        string? command,
        string[]? args)
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);

        sb.Append(command);

        if (args?.Length > 0)
        {
            for (var i = 0; i < args.Length; i++)
            {
                sb.Append(Space);
                sb.Append(args[i]);
            }
        }

        return sb.ToString();
    }

    private static void EscapeSingleArg(ref ValueStringBuilder sb, ReadOnlySpan<char> arg)
    {
        var needsQuotes = arg.Length == 0 || ArgumentContainsWhitespace(arg);
        var isQuoted = needsQuotes || IsSurroundedWithQuotes(arg);

        if (needsQuotes)
        {
            sb.Append(Quote);
        }

        for (var i = 0; i < arg.Length; ++i)
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
    /// <param name="quoteValue">If the value should be wrapped in quotes if it contains whitespace.</param>
    private static void EscapeArgForCmd(ref ValueStringBuilder sb, ReadOnlySpan<char> argument, bool quoteValue)
    {
        var quoted = quoteValue && ArgumentContainsWhitespace(argument);

        if (quoted)
        {
            sb.Append(Caret);
            sb.Append(Quote);
        }

        // Prepend every character with ^
        // This is harmless when passing through cmd
        // and ensures cmd metacharacters are not interpreted
        // as such
        for (var i = 0; i < argument.Length; i++)
        {
            sb.Append(Caret);
            sb.Append(argument[i]);
        }

        if (quoted)
        {
            sb.Append(Caret);
            sb.Append(Quote);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSurroundedWithQuotes(ReadOnlySpan<char> argument)
        => argument[0] == Quote &&
            argument[^1] == Quote;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ArgumentContainsWhitespace(ReadOnlySpan<char> argument)
        => argument.IndexOfAny(Space, Tab, NewLine) >= 0;
}
