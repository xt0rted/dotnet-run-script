namespace RunScript;

using System.Text;

// https://github.com/dotnet/sdk/blob/a758a468b71e15303198506a8de1040649aa0f35/src/Cli/Microsoft.DotNet.Cli.Utils/StreamForwarder.cs
internal sealed class StreamForwarder
{
    private static readonly char[] _ignoreCharacters = { '\r' };

    private const char FlushBuilderCharacter = '\n';

    private StringBuilder? _builder;
#pragma warning disable IDISP006 // Implement IDisposable
    private StringWriter? _capture;
#pragma warning restore IDISP006 // Implement IDisposable
    private Action<string>? _writeLine;
    private bool _trimTrailingCapturedNewline;

    public string? CapturedOutput
    {
        get
        {
            var capture = _capture?.GetStringBuilder()?.ToString();

            if (_trimTrailingCapturedNewline)
            {
                capture = capture?.TrimEnd('\r', '\n');
            }

            return capture;
        }
    }

    public StreamForwarder Capture(bool trimTrailingNewline = false)
    {
        ThrowIfCaptureSet();

        _capture?.Dispose();
        _capture = new StringWriter();
        _trimTrailingCapturedNewline = trimTrailingNewline;

        return this;
    }

    public StreamForwarder ForwardTo(Action<string> writeLine)
    {
        ThrowIfNull(writeLine);

        ThrowIfForwarderSet();

        _writeLine = writeLine;

        return this;
    }

    public Task BeginReadAsync(TextReader reader)
        => Task.Run(() => Read(reader));

    public void Read(TextReader reader)
    {
        if (reader is null) throw new ArgumentNullException(nameof(reader));

        const int bufferSize = 1;

        char currentCharacter;

        var buffer = new char[bufferSize];
        _builder = new StringBuilder();

        // Using Read with buffer size 1 to prevent looping endlessly
        // like we would when using Read() with no buffer
        while ((_ = reader.Read(buffer, 0, bufferSize)) > 0)
        {
            currentCharacter = buffer[0];

            if (currentCharacter == FlushBuilderCharacter)
            {
                WriteBuilder();
            }
            else if (!_ignoreCharacters.Contains(currentCharacter))
            {
                _builder.Append(currentCharacter);
            }
        }

        // Flush anything else when the stream is closed
        // Which should only happen if someone used console.Write
        if (_builder.Length > 0)
        {
            WriteBuilder();
        }
    }

    private void WriteBuilder()
    {
        if (_builder is not null)
        {
            WriteLine(_builder.ToString());
            _builder.Clear();
        }
    }

    private void WriteLine(string str)
    {
        _capture?.WriteLine(str);

        if (_writeLine is not null)
        {
            _writeLine(str);
        }
    }

    private void ThrowIfNull(object obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
    }

    private void ThrowIfForwarderSet()
    {
        if (_writeLine is not null)
        {
            throw new InvalidOperationException("WriteLine forwarder set previously");
        }
    }

    private void ThrowIfCaptureSet()
    {
        if (_capture is not null)
        {
            throw new InvalidOperationException("Already capturing stream!");
        }
    }
}
