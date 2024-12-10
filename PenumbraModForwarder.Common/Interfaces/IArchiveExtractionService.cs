using PenumbraModForwarder.Common.Events;
using SevenZipExtractor;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IArchiveExtractionService
{
    event EventHandler<ExtractProgressProp> ExtractProgress;
    event EventHandler<MultipleEntriesEventArgs> MultipleEntriesFound;

    Task ExtractAsync(string archivePath, CancellationToken cancellationToken = default);
}