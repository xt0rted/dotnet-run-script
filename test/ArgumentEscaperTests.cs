namespace RunScript;

// https://github.com/dotnet/sdk/blob/09b31215867d1ffe4955fd5b7cd91eb552d3632c/src/Tests/Microsoft.DotNet.Cli.Utils.Tests/ArgumentEscaperTests.cs
public class ArgumentEscaperTests
{
    [Theory]
    [InlineData(new[] { "one", "two", "three" }, " one two three")]
    [InlineData(new[] { "line1\nline2", "word1\tword2" }, " \"line1\nline2\" \"word1\tword2\"")]
    [InlineData(new[] { "with spaces" }, " \"with spaces\"")]
    [InlineData(new[] { @"with\backslash" }, @" with\backslash")]
    [InlineData(new[] { @"""quotedwith\backslash""" }, @" \""quotedwith\backslash\""")]
    [InlineData(new[] { @"C:\Users\" }, @" C:\Users\")]
    [InlineData(new[] { @"C:\Program Files\dotnet\" }, @" ""C:\Program Files\dotnet\\""")]
    [InlineData(new[] { @"backslash\""preceedingquote" }, @" backslash\\\""preceedingquote")]
    [InlineData(new[] { @""" hello """ }, @" ""\"" hello \""""")]
    public void EscapesArgumentsForProcessStart(string[] args, string expected)
    {
        // Given / When
        var result = ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(args);

        // Then
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(new[] { "one", "two", "three" }, " ^o^n^e ^t^w^o ^t^h^r^e^e")]
    [InlineData(new[] { "line1\nline2", "word1\tword2" }, " ^\"^l^i^n^e^1^\n^l^i^n^e^2^\" ^\"^w^o^r^d^1^\t^w^o^r^d^2^\"")]
    [InlineData(new[] { "with spaces" }, " ^\"^w^i^t^h^ ^s^p^a^c^e^s^\"")]
    [InlineData(new[] { @"with\backslash" }, @" ^w^i^t^h^\^b^a^c^k^s^l^a^s^h")]
    [InlineData(new[] { @"""quotedwith\backslash""" }, @" ^""^q^u^o^t^e^d^w^i^t^h^\^b^a^c^k^s^l^a^s^h^""")]
    [InlineData(new[] { @"C:\Users\" }, @" ^C^:^\^U^s^e^r^s^\")]
    [InlineData(new[] { @"C:\Program Files\dotnet\" }, @" ^""^C^:^\^P^r^o^g^r^a^m^ ^F^i^l^e^s^\^d^o^t^n^e^t^\^""")]
    [InlineData(new[] { @"backslash\""preceedingquote" }, @" ^b^a^c^k^s^l^a^s^h^\^""^p^r^e^c^e^e^d^i^n^g^q^u^o^t^e")]
    [InlineData(new[] { @""" hello """ }, @" ^""^""^ ^h^e^l^l^o^ ^""^""")]
    public void EscapeAndConcatenateArgArrayForCmdProcessStart(string[] args, string expected)
    {
        // Given / When
        var result = ArgumentEscaper.EscapeAndConcatenateArgArrayForCmdProcessStart(args);

        // Then
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(new[] { "one", "two", "three" }, "c m d one two three")]
    [InlineData(new[] { "line1\nline2", "word1\tword2" }, "c m d line1\nline2 word1\tword2")]
    [InlineData(new[] { "with spaces" }, "c m d with spaces")]
    [InlineData(new[] { @"with\backslash" }, @"c m d with\backslash")]
    [InlineData(new[] { @"""quotedwith\backslash""" }, @"c m d ""quotedwith\backslash""")]
    [InlineData(new[] { @"C:\Users\" }, @"c m d C:\Users\")]
    [InlineData(new[] { @"C:\Program Files\dotnet\" }, @"c m d C:\Program Files\dotnet\")]
    [InlineData(new[] { @"backslash\""preceedingquote" }, @"c m d backslash\""preceedingquote")]
    [InlineData(new[] { @""" hello """ }, @"c m d "" hello """)]
    public void ConcatinateCommandAndArgArrayForDisplay(string[] args, string expected)
    {
        // Given / When
        var result = ArgumentEscaper.ConcatinateCommandAndArgArrayForDisplay("c m d", args);

        // Then
        result.ShouldBe(expected);
    }
}
