using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;
using CustomWebSocketMessageType = PenumbraModForwarder.Common.Models.WebSocketMessageType;

namespace PenumbraModForwarder.UI.Services
{
    public class WebSocketClient : IWebSocketClient, IDisposable
    {
        private readonly Dictionary<string, ClientWebSocket> _webSockets;
        private readonly INotificationService _notificationService;
        private readonly CancellationTokenSource _cts = new();
        private readonly string[] _endpoints = { "/status", "/currentTask", "/config", "/install" };
        private readonly ILogger _logger;
        private bool _isReconnecting;
        private int _retryCount = 0;

        public WebSocketClient(INotificationService notificationService)
        {
            _webSockets = new Dictionary<string, ClientWebSocket>();
            _notificationService = notificationService;
            _logger = Log.ForContext<WebSocketClient>();
        }

        public async Task ConnectAsync(int port)
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await ConnectEndpointsAsync(port);
                    _retryCount = 0;
                    await Task.Delay(1000, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _retryCount++;
                    _logger.Error(ex, "Connection loop error. Retry attempt: {RetryCount}", _retryCount);
                    await _notificationService.ShowNotification(
                        $"Connection failed. Retrying in 5 seconds... (Attempt {_retryCount})",
                        5
                    );
                    await Task.Delay(5000, _cts.Token);
                }
            }
        }

        private async Task ConnectEndpointsAsync(int port)
        {
            foreach (var endpoint in _endpoints)
            {
                try
                {
                    if (!_webSockets.ContainsKey(endpoint) || _webSockets[endpoint].State != WebSocketState.Open)
                    {
                        if (_webSockets.ContainsKey(endpoint))
                        {
                            await DisconnectWebSocketAsync(_webSockets[endpoint]);
                        }

                        var webSocket = new ClientWebSocket();
                        _webSockets[endpoint] = webSocket;

                        await webSocket.ConnectAsync(
                            new Uri($"ws://localhost:{port}{endpoint}"),
                            _cts.Token
                        );

                        if (endpoint == "/config")
                        {
                            // If needed, start receiving messages on /config endpoint
                            // _ = ReceiveMessagesAsync(webSocket, endpoint);
                        }
                        else
                        {
                            _ = ReceiveMessagesAsync(webSocket, endpoint);
                        }

                        if (_isReconnecting)
                        {
                            _isReconnecting = false;
                            await _notificationService.ShowNotification("Connection restored successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!_isReconnecting)
                    {
                        _logger.Error(ex, "WebSocket connection failed for endpoint {Endpoint}. Attempting to reconnect...", endpoint);
                        await _notificationService.ShowNotification(
                            $"Connection to {endpoint} lost. Attempting to reconnect...",
                            5
                        );
                        _isReconnecting = true;
                    }
                    throw; // Propagate to main loop for retry
                }
            }
        }

        public async Task SendMessageAsync(WebSocketMessage message, string endpoint)
        {
            if (_webSockets.TryGetValue(endpoint, out var webSocket))
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var json = JsonConvert.SerializeObject(message);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    try
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
                        _logger.Debug("Sent message to endpoint {Endpoint}: {Message}", endpoint, json);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error sending WebSocket message to {Endpoint}", endpoint);
                    }
                }
                else
                {
                    _logger.Warning("WebSocket to {Endpoint} is not open", endpoint);
                }
            }
            else
            {
                _logger.Warning("No WebSocket connection found for endpoint {Endpoint}", endpoint);
            }
        }

        private async Task DisconnectWebSocketAsync(ClientWebSocket webSocket)
        {
            try
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Disconnecting for reconnection",
                        _cts.Token
                    );
                }
                webSocket.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during WebSocket disconnect");
                await _notificationService.ShowNotification("Error disconnecting WebSocket connection");
            }
        }

        private async Task ReceiveMessagesAsync(ClientWebSocket webSocket, string endpoint)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cts.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var message = JsonConvert.DeserializeObject<WebSocketMessage>(messageJson);

                        _logger.Information("Received message from {Endpoint}: {Message}", endpoint, messageJson);

                        // Handle messages based on endpoint
                        switch (endpoint)
                        {
                            case "/install":
                                await HandleInstallMessageAsync(message);
                                break;
                            default:
                                await HandleGeneralMessageAsync(message, endpoint);
                                break;
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectWebSocketAsync(webSocket);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error receiving WebSocket messages from {Endpoint}", endpoint);
                if (!_isReconnecting)
                {
                    await _notificationService.ShowNotification(
                        $"Lost connection to {endpoint}. Attempting to reconnect..."
                    );
                    _isReconnecting = true;
                }
            }
        }

        private async Task HandleInstallMessageAsync(WebSocketMessage message)
        {
            if (message.Type == CustomWebSocketMessageType.Status && message.Status == "select_files")
            {
                // Log received message
                _logger.Information("Received 'select_files' message: {Message}", message.Message);

                // Deserialize the list of files
                var fileList = JsonConvert.DeserializeObject<List<string>>(message.Message);

                // TODO: For now, we will mock the user selection by selecting all files
                var selectedFiles = fileList; // Mock selection: select all files
                
                var responseMessage = new WebSocketMessage
                {
                    Type = CustomWebSocketMessageType.Status,
                    TaskId = message.TaskId,
                    Status = "user_selection",
                    Progress = 0,
                    Message = JsonConvert.SerializeObject(selectedFiles)
                };
                
                await SendMessageAsync(responseMessage, "/install");

                _logger.Information("Sent 'user_selection' message with selected files");
            }
            else
            {
                _logger.Warning("Unhandled message type or status on /install endpoint: Type={Type}, Status={Status}", message.Type, message.Status);
            }
        }

        private async Task HandleGeneralMessageAsync(WebSocketMessage message, string endpoint)
        {
            if (message.Type == CustomWebSocketMessageType.Status ||
                message.Type == CustomWebSocketMessageType.Progress)
            {
                if (message.Progress > 0)
                {
                    _notificationService.UpdateProgress(
                        message.TaskId,
                        message.Message,
                        message.Progress
                    );
                }
                else
                {
                    await _notificationService.ShowNotification(message.Message);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            foreach (var webSocket in _webSockets.Values)
            {
                DisconnectWebSocketAsync(webSocket).Wait();
            }
            _cts.Dispose();
        }
    }
}