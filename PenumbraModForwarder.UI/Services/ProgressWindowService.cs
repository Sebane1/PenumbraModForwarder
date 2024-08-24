using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Services
{
    public class ProgressWindowService : IProgressWindowService
    {
        private readonly ILogger<ProgressWindowViewModel> _progressWindowLogger;
        private readonly ILogger<ProgressWindowService> _logger;
        private ProgressWindow _progressWindow;

        public ProgressWindowService(ILogger<ProgressWindowViewModel> progressWindowLogger, ILogger<ProgressWindowService> logger)
        {
            _progressWindowLogger = progressWindowLogger;
            _logger = logger;
        }

        public void ShowProgressWindow()
        {
            _logger.LogDebug("Showing progress window.");
            if (Application.OpenForms.Count > 0)
            {
                // Ensure we use the UI thread for showing the form
                Application.OpenForms[0].Invoke(() => ShowProgressWindowInternal());
            }
            else
            {
                ShowProgressWindowInternal();
            }
        }

        public void UpdateProgress(string fileName, string operation, int progress)
        {
            _logger.LogDebug("Updating progress window with {FileName}, {Operation}, {Progress}.", fileName, operation, progress);
            if (_progressWindow is {InvokeRequired: true})
            {
                // Ensure we use the UI thread for updating the form
                _progressWindow.Invoke(() =>
                {
                    _progressWindow.ViewModel.FileName = fileName;
                    _progressWindow.ViewModel.Operation = operation;
                    _progressWindow.ViewModel.Progress = progress;
                });
            }
            else
            {
                _progressWindow.ViewModel.FileName = fileName;
                _progressWindow.ViewModel.Operation = operation;
                _progressWindow.ViewModel.Progress = progress;
            }
        }

        public void CloseProgressWindow()
        {
            if (_progressWindow is {InvokeRequired: true})
            {
                _progressWindow.Invoke(() => _progressWindow.Close());
            }
            else
            {
                _progressWindow.Close();
            }
        }

        private void ShowProgressWindowInternal()
        {
            // Create the ProgressWindow without blocking the UI thread
            _progressWindow = new ProgressWindow(new ProgressWindowViewModel(_progressWindowLogger));
            _progressWindow.Show();
        }
    }
}
