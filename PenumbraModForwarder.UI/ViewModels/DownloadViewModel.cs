using Serilog;

namespace PenumbraModForwarder.UI.ViewModels;

public class DownloadViewModel : ViewModelBase
{
    private ILogger _logger;
    public DownloadViewModel()
    {
        _logger = Log.ForContext<DownloadViewModel>();
    }
}