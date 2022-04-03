#pragma warning disable RCS1162 // Avoid chain of assignments.

namespace RunScript;

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
/// This is a stripped down version of
/// <seealso href="https://github.com/dotnet/runtime/blob/34f2b587f3512d3974d73b316e565194df34eea3/src/libraries/Common/src/System/Text/ValueStringBuilder.cs"/>
/// with only the minimal parts needed for our use.
/// </summary>
internal ref struct ValueStringBuilder
{
    private char[]? _arrayToReturnToPool;
    private Span<char> _chars;
    private int _pos;

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        _arrayToReturnToPool = null;
        _chars = initialBuffer;
        _pos = 0;
    }

    public int Length => _pos;

    public override string ToString()
    {
        var s = _chars[.._pos].ToString();

        Dispose();

        return s;
    }

    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (_chars.Slice(0, _pos).TryCopyTo(destination))
        {
            charsWritten = _pos;
            Dispose();

            return true;
        }

        charsWritten = 0;
        Dispose();

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        var pos = _pos;

        if ((uint)pos < (uint)_chars.Length)
        {
            _chars[pos] = c;
            _pos = pos + 1;
        }
        else
        {
            GrowAndAppend(c);
        }
    }

    public void Append(char c, int count)
    {
        if (_pos > _chars.Length - count)
        {
            Grow(count);
        }

        var dst = _chars.Slice(_pos, count);

        for (var i = 0; i < dst.Length; i++)
        {
            dst[i] = c;
        }

        _pos += count;
    }

    public void Append(ReadOnlySpan<char> value)
    {
        var pos = _pos;

        if (pos > _chars.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_chars[_pos..]);

        _pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(1);
        Append(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
        var poolArray = ArrayPool<char>.Shared.Rent((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_chars.Length * 2));

        _chars[.._pos].CopyTo(poolArray);

        var toReturn = _arrayToReturnToPool;

        _chars = _arrayToReturnToPool = poolArray;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        var toReturn = _arrayToReturnToPool;

        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}
