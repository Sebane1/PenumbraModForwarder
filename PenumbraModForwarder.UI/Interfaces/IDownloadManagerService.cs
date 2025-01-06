using System.Threading;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IDownloadManagerService
{
    Task DownloadModsAsync(XmaMods mod, CancellationToken ct);
}