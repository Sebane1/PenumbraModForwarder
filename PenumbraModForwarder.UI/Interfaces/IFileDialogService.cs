using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IFileDialogService
{
    Task<string> OpenFolderAsync(string initialDirectory, string title);
    Task<IEnumerable<string>> OpenFoldersAsync(string initialDirectory, string title);
}