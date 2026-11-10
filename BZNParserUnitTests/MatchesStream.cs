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
    /// Trailing CR/LF bytes in the comparison stream are tolerated — the source file may have
    /// extra newlines at the very tail that the writer doesn't reproduce.
    /// </summary>
    public void AssertComplete()
    {
        long remaining = _compareStream.Length - _position;
        if (remaining <= 0)
            return;

        // Allow trailing newlines (CR/LF) that the writer didn't emit
        byte[] tail = new byte[remaining];
        long savedPos = _compareStream.Position;
        _compareStream.Seek(_position, SeekOrigin.Begin);
        int read = _compareStream.Read(tail, 0, (int)remaining);
        _compareStream.Position = savedPos;

        for (int i = 0; i < read; i++)
        {
            if (tail[i] != 0x0D && tail[i] != 0x0A)
                throw new InvalidOperationException(
                    $"Incomplete write: wrote {_position} of {_compareStream.Length} bytes " +
                    $"(missing {_compareStream.Length - _position} trailing bytes, " +
                    $"first non-newline byte at offset {_position + i} = 0x{tail[i]:X2}).");
        }
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