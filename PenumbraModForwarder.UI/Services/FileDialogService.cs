using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.UI.Services;

public class FileDialogService : IFileDialogService
{
    private readonly Window _parentWindow;

    public FileDialogService(Window parentWindow)
    {
        _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
    }

    public async Task<string> OpenFolderAsync(string initialDirectory, string title)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = !string.IsNullOrEmpty(initialDirectory)
                ? await _parentWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory)
                : null
        };

        var result = await _parentWindow.StorageProvider.OpenFolderPickerAsync(options);
        return result?.FirstOrDefault()?.Path?.LocalPath;
    }

    public async Task<IEnumerable<string>> OpenFoldersAsync(string initialDirectory, string title)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = true,
            SuggestedStartLocation = !string.IsNullOrEmpty(initialDirectory)
                ? await _parentWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory)
                : null
        };

        var results = await _parentWindow.StorageProvider.OpenFolderPickerAsync(options);
        return results?.Select(r => r.Path?.LocalPath);
    }
}