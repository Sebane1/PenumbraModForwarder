namespace PenumbraModForwarder.Common.Helpers;

public class ProgressStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _totalBytes;
    private readonly IProgress<double> _progress;
    private long _bytesWritten;

    public ProgressStream(Stream baseStream, long totalBytes, IProgress<double> progress)
    {
        _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        _totalBytes = totalBytes;
        _progress = progress ?? throw new ArgumentNullException(nameof(progress));
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _baseStream.Write(buffer, offset, count);
        _bytesWritten += count;

        // Report progress as a percentage
        var percentage = (double)_bytesWritten / _totalBytes * 100;
        _progress.Report(percentage);
    }

    // Pass-through implementations for the other Stream methods
    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _baseStream.Length;
    public override long Position 
    { 
        get => _baseStream.Position; 
        set => _baseStream.Position = value; 
    }

    public override void Flush() => _baseStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
    public override void SetLength(long value) => _baseStream.SetLength(value);
}