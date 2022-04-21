namespace RunScript;

// https://github.com/dotnet/sdk/blob/09b31215867d1ffe4955fd5b7cd91eb552d3632c/src/Tests/Microsoft.DotNet.Cli.Utils.Tests/ArgumentEscaperTests.cs
public class ArgumentBuilderTests
{
    [Theory]
    [InlineData("cmd", null, "cmd")]
    [InlineData("cm \"d\"", null, "cm \"d\"")]
    [InlineData("c m d", null, "c m d")]
    [InlineData("c m d", new string[0], "c m d")]
    [InlineData("c m d", new[] { "one", "two", "three" }, "c m d one two three")]
    [InlineData("c m d", new[] { "line1\nline2", "word1\tword2" }, "c m d \"line1\nline2\" \"word1\tword2\"")]
    [InlineData("c m d", new[] { "with spaces" }, "c m d \"with spaces\"")]
    [InlineData("c m d", new[] { @"with\backslash" }, @"c m d with\backslash")]
    [InlineData("c m d", new[] { @"""quotedwith\backslash""" }, @"c m d \""quotedwith\backslash\""")]
    [InlineData("c m d", new[] { @"C:\Users\" }, @"c m d C:\Users\")]
    [InlineData("c m d", new[] { @"C:\Program Files\dotnet\" }, @"c m d ""C:\Program Files\dotnet\\""")]
    [InlineData("c m d", new[] { @"backslash\""preceedingquote" }, @"c m d backslash\\\""preceedingquote")]
    [InlineData("c m d", new[] { @""" hello """ }, @"c m d ""\"" hello \""""")]
    public void EscapeAndConcatenateCommandAndArgArrayForProcessStart(string command, string[] args, string expected)
    {
        // Given / When
        var result = ArgumentBuilder.EscapeAndConcatenateCommandAndArgArrayForProcessStart(command, args);

        // Then
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("cmd", null, "^c^m^d")]
    [InlineData("cm \"d\"", null, "^c^m^ ^\"^d^\"")]
    [InlineData("c m d", null, "^c^ ^m^ ^d")]
    [InlineData("c m d", new string[0], "^c^ ^m^ ^d")]
    [InlineData("c m d", new[] { "one", "two", "three" }, "^c^ ^m^ ^d^ ^o^n^e^ ^t^w^o^ ^t^h^r^e^e")]
    [InlineData("c m d", new[] { "line1\nline2", "word1\tword2" }, "^c^ ^m^ ^d^ ^\"^l^i^n^e^1^\n^l^i^n^e^2^\"^ ^\"^w^o^r^d^1^\t^w^o^r^d^2^\"")]
    [InlineData("c m d", new[] { "with spaces" }, "^c^ ^m^ ^d^ ^\"^w^i^t^h^ ^s^p^a^c^e^s^\"")]
    [InlineData("c m d", new[] { @"with\backslash" }, @"^c^ ^m^ ^d^ ^w^i^t^h^\^b^a^c^k^s^l^a^s^h")]
    [InlineData("c m d", new[] { @"""quotedwith\backslash""" }, @"^c^ ^m^ ^d^ ^""^q^u^o^t^e^d^w^i^t^h^\^b^a^c^k^s^l^a^s^h^""")]
    [InlineData("c m d", new[] { @"C:\Users\" }, @"^c^ ^m^ ^d^ ^C^:^\^U^s^e^r^s^\")]
    [InlineData("c m d", new[] { @"C:\Program Files\dotnet\" }, @"^c^ ^m^ ^d^ ^""^C^:^\^P^r^o^g^r^a^m^ ^F^i^l^e^s^\^d^o^t^n^e^t^\^""")]
    [InlineData("c m d", new[] { @"backslash\""preceedingquote" }, @"^c^ ^m^ ^d^ ^b^a^c^k^s^l^a^s^h^\^""^p^r^e^c^e^e^d^i^n^g^q^u^o^t^e")]
    [InlineData("c m d", new[] { @""" hello """ }, @"^c^ ^m^ ^d^ ^""^""^ ^h^e^l^l^o^ ^""^""")]
    public void EscapeAndConcatenateCommandAndArgArrayForCmdProcessStart(string command, string[] args, string expected)
    {
        // Given / When
        var result = ArgumentBuilder.EscapeAndConcatenateCommandAndArgArrayForCmdProcessStart(command, args);

        // Then
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "c m d")]
    [InlineData(new string[0], "c m d")]
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
        var result = ArgumentBuilder.ConcatinateCommandAndArgArrayForDisplay("c m d", args);

        // Then
        result.ShouldBe(expected);
    }
}
