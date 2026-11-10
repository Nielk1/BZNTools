using System;
using System.IO;

public class MatchesStream : Stream
{
    private readonly Stream _compareStream;
    private long _position;

    public MatchesStream(Stream compareStream)
    {
        _compareStream = compareStream ?? throw new ArgumentNullException(nameof(compareStream));
        if (!_compareStream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(compareStream));
        _position = 0;
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _compareStream.Length;
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override void Flush() { }

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    /// <summary>
    /// True when the entire comparison stream has been consumed by writes.
    /// </summary>
    public bool IsComplete => _position >= _compareStream.Length;

    /// <summary>
    /// Throws if the write run was incomplete (fewer bytes written than the comparison stream holds).
    /// An incomplete run would otherwise look valid because Write() only detects an over-long write.
    /// </summary>
    public void AssertComplete()
    {
        if (_position < _compareStream.Length)
            throw new InvalidOperationException(
                $"Incomplete write: wrote {_position} of {_compareStream.Length} bytes " +
                $"(missing {_compareStream.Length - _position} trailing bytes).");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        byte[] compareBuffer = new byte[count];
        int read = _compareStream.Read(compareBuffer, 0, count);
        if (read != count)
            throw new EndOfStreamException("Target stream is shorter than written data.");

        for (int i = 0; i < count; i++)
        {
            if (buffer[offset + i] != compareBuffer[i])
                 throw new InvalidOperationException($"Byte mismatch at position {_position + i}.");
        }
        _position += count;
    }
}