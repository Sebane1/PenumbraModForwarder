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
        private readonly object _lock = new object();

        public ProgressWindowService(ILogger<ProgressWindowViewModel> progressWindowLogger, ILogger<ProgressWindowService> logger)
        {
            _progressWindowLogger = progressWindowLogger;
            _logger = logger;
        }

        public void ShowProgressWindow()
        {
            lock (_lock)
            {
                if (IsProgressWindowAvailable())
                {
                    return;
                }

                ShowProgressWindowInternal();
            }
        }

        public void UpdateProgress(string fileName, string operation, int progress)
        {
            lock (_lock)
            {
                if (!IsProgressWindowAvailable())
                {
                    _logger.LogWarning("Progress window is null or disposed, cannot update progress.");
                    return;
                }

                UpdateProgressInternal(fileName, operation, progress);
            }
        }

        public void CloseProgressWindow()
        {
            lock (_lock)
            {
                if (!IsProgressWindowAvailable())
                {
                    return;
                }

                CloseProgressWindowInternal();
            }
        }

        private bool IsProgressWindowAvailable()
        {
            return _progressWindow is {IsDisposed: false};
        }

        private void ShowProgressWindowInternal()
        {
            if (Application.OpenForms.Count > 0)
            {
                Application.OpenForms[0].Invoke(CreateAndShowProgressWindow);
            }
            else
            {
                CreateAndShowProgressWindow();
            }
        }

        private void CreateAndShowProgressWindow()
        {
            _progressWindow = new ProgressWindow(new ProgressWindowViewModel(_progressWindowLogger));
            _progressWindow.Show();
        }

        private void UpdateProgressInternal(string fileName, string operation, int progress)
        {
            try
            {
                if (_progressWindow.InvokeRequired)
                {
                    _progressWindow.Invoke(() => SetProgress(fileName, operation, progress));
                }
                else
                {
                    SetProgress(fileName, operation, progress);
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogWarning(ex, "Progress window was disposed while attempting to update progress.");
            }
        }

        private void SetProgress(string fileName, string operation, int progress)
        {
            if (!_progressWindow.IsDisposed)
            {
                _progressWindow.ViewModel.FileName = fileName;
                _progressWindow.ViewModel.Operation = operation;
                _progressWindow.ViewModel.Progress = progress;
            }
        }

        private void CloseProgressWindowInternal()
        {
            try
            {
                if (_progressWindow.InvokeRequired)
                {
                    _progressWindow.Invoke(() =>
                    {
                        if (!_progressWindow.IsDisposed)
                        {
                            _progressWindow.Close();
                            _progressWindow = null;
                        }
                    });
                }
                else
                {
                    _progressWindow.Close();
                    _progressWindow = null;
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogWarning(ex, "Progress window was disposed while attempting to close it.");
            }
        }
    }
}
