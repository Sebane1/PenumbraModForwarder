using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;
using SevenZipExtractor;

namespace PenumbraModForwarder.Common.Services;

public class ArchiveExtractionService : IArchiveExtractionService
{
    public event EventHandler<ExtractProgressProp> ExtractProgress;
    
    public event EventHandler<MultipleEntriesEventArgs> MultipleEntriesFound;

    public async Task ExtractAsync(string archivePath, CancellationToken cancellationToken = default)
    {
        using var archiveFile = new ArchiveFile(archivePath);
        
        if (archiveFile.Entries.Count > 1)
        {
            InformMultipleEntries(archiveFile.Entries);
            return;
        }
        
        await ExtractEntriesAsync(archiveFile, archivePath, filesToExtract: null, cancellationToken);
    }

    public async Task ExtractAsync(string archivePath, IEnumerable<string> filesToExtract, CancellationToken cancellationToken = default)
    {
        using var archiveFile = new ArchiveFile(archivePath);

        await ExtractEntriesAsync(archiveFile, archivePath, filesToExtract, cancellationToken);
    }

    private async Task ExtractEntriesAsync(ArchiveFile archiveFile, string archivePath, IEnumerable<string> filesToExtract, CancellationToken cancellationToken)
    {
        var outputDirectory = Path.GetDirectoryName(archivePath);
        if (string.IsNullOrEmpty(outputDirectory))
        {
            Log.Error("Output directory could not be determined for extraction.");
            return;
        }
        
        archiveFile.ExtractProgress += OnExtractProgress;

        try
        {
            await Task.Run(() =>
            {
                archiveFile.Extract(entry =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (entry == null)
                        return null;
                    
                    if (filesToExtract != null && !filesToExtract.Contains(entry.FileName))
                    {
                        return null;
                    }

                    var extension = Path.GetExtension(entry.FileName);

                    if (!FileExtensionsConsts.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                        return null;
                    var outputFilePath = Path.Combine(outputDirectory, entry.FileName);
                        
                    var directoryName = Path.GetDirectoryName(outputFilePath);
                    if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                        
                    return outputFilePath;

                }, cancellationToken);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Log.Information("Extraction was canceled by the user.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred during extraction.");
        }
        finally
        {
            // Unsubscribe from the event and clean up
            archiveFile.ExtractProgress -= OnExtractProgress;
            
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }
        }
    }

    private void OnExtractProgress(object sender, ExtractProgressProp e)
    {
        ExtractProgress?.Invoke(this, e);
    }

    private void InformMultipleEntries(IEnumerable<Entry> entries)
    {
        var entryNames = entries.Select(entry => entry.FileName).ToList();

        // Raise the event with the list of entries
        MultipleEntriesFound?.Invoke(this, new MultipleEntriesEventArgs(entryNames));

        Log.Information("The archive contains multiple entries.");
    }
}