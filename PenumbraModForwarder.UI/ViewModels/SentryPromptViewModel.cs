using System;
using System.Reactive;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels
{
    public class SentryPromptViewModel : ViewModelBase, IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IConfigurationService _configurationService;
        private readonly IWebSocketClient _webSocketClient;

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }

        public ReactiveCommand<Unit, Unit> AcceptCommand { get; }
        public ReactiveCommand<Unit, Unit> DeclineCommand { get; }

        public SentryPromptViewModel(IConfigurationService configurationService, IWebSocketClient webSocketClient)
        {
            _configurationService = configurationService;
            _webSocketClient = webSocketClient;

            AcceptCommand = ReactiveCommand.CreateFromTask(ExecuteAcceptCommand);
            DeclineCommand = ReactiveCommand.CreateFromTask(ExecuteDeclineCommand);
        }

        private async Task ExecuteAcceptCommand()
        {
            _logger.Info("User accepted Sentry.");

            _configurationService.UpdateConfigValue(
                c => c.Common.EnableSentry = true,
                "Common.EnableSentry",
                true
            );
            _configurationService.UpdateConfigValue(
                c => c.Common.UserChoseSentry = true,
                "Common.UserChoseSentry",
                true
            );
            
            await SendConfigurationChangeAsync("Common.EnableSentry", true);
            await SendConfigurationChangeAsync("Common.UserChoseSentry", true);

            IsVisible = false;
        }

        private async Task ExecuteDeclineCommand()
        {
            _logger.Info("User declined Sentry.");

            _configurationService.UpdateConfigValue(
                c => c.Common.EnableSentry = false,
                "Common.EnableSentry",
                false
            );
            _configurationService.UpdateConfigValue(
                c => c.Common.UserChoseSentry = true,
                "Common.UserChoseSentry",
                true
            );
            
            await SendConfigurationChangeAsync("Common.EnableSentry", false);
            await SendConfigurationChangeAsync("Common.UserChoseSentry", true);

            IsVisible = false;
        }

        private async Task SendConfigurationChangeAsync(string propertyPath, object value)
        {
            var taskId = Guid.NewGuid().ToString();
            var configurationChange = new
            {
                PropertyPath = propertyPath,
                NewValue = value
            };

            var message = WebSocketMessage.CreateStatus(
                taskId,
                WebSocketMessageStatus.InProgress,
                $"Configuration changed: {propertyPath}"
            );

            message.Type = WebSocketMessageType.ConfigurationChange;
            message.Message = JsonConvert.SerializeObject(configurationChange);

            // Fire and forget the send operation
            _ = _webSocketClient.SendMessageAsync(message, "/config").ConfigureAwait(false);

            // If needed, await the task:
            // await _webSocketClient.SendMessageAsync(message, "/config");
        }

        public void Dispose()
        {
        }
    }
}