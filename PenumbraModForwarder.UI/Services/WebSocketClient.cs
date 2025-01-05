using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Events;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;
using CustomWebSocketMessageType = PenumbraModForwarder.Common.Models.WebSocketMessageType;

namespace PenumbraModForwarder.UI.Services;

public class WebSocketClient : IWebSocketClient, IDisposable
{
    private readonly Dictionary<string, ClientWebSocket> _webSockets;
    private readonly INotificationService _notificationService;
    private readonly IConfigurationService _configurationService;
    private readonly CancellationTokenSource _cts = new();
    private readonly string[] _endpoints = { "/status", "/currentTask", "/config", "/install" };
    private readonly ILogger _logger;
    private bool _isReconnecting;
    private int _retryCount;
    
    private readonly string _clientId;

    public event EventHandler<FileSelectionRequestedEventArgs> FileSelectionRequested;

    public WebSocketClient(INotificationService notificationService, IConfigurationService configurationService)
    {
        _webSockets = new Dictionary<string, ClientWebSocket>();
        _notificationService = notificationService;
        _configurationService = configurationService;
        _logger = Log.ForContext<WebSocketClient>();

        // Assign a unique ID to help identify outgoing messages
        _clientId = Guid.NewGuid().ToString("N");
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
                    SoundType.GeneralChime,
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

                    _ = ReceiveMessagesAsync(webSocket, endpoint);

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
                        SoundType.GeneralChime,
                        5
                    );
                    _isReconnecting = true;
                }
                throw;
            }
        }
    }

    public async Task SendMessageAsync(WebSocketMessage message, string endpoint)
    {
        message.ClientId = _clientId;

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

                    // Ignore messages that match our own client ID
                    if (message?.ClientId == _clientId)
                    {
                        _logger.Debug("Ignored message from this client: {MessageJson}", messageJson);
                        continue;
                    }

                    _logger.Information("Received message from {Endpoint}: {Message}", endpoint, messageJson);

                    switch (endpoint)
                    {
                        case "/install":
                            await HandleInstallMessageAsync(message);
                            break;
                        case "/config":
                            await HandleConfigMessageAsync(message);
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
                    $"Lost connection to {endpoint}. Attempting to reconnect...",
                    SoundType.GeneralChime
                );
                _isReconnecting = true;
            }
        }
    }

    private async Task HandleInstallMessageAsync(WebSocketMessage message)
    {
        if (message.Type == CustomWebSocketMessageType.Status && message.Status == "select_files")
        {
            _logger.Information("Received 'select_files' message: {Message}", message.Message);

            var fileList = JsonConvert.DeserializeObject<List<string>>(message.Message);
            FileSelectionRequested?.Invoke(this, new FileSelectionRequestedEventArgs(fileList, message.TaskId));
        }
        else
        {
            _logger.Warning(
                "Unhandled message on /install endpoint: Type={Type}, Status={Status}",
                message.Type,
                message.Status
            );
        }
    }

    private async Task HandleConfigMessageAsync(WebSocketMessage message)
    {
        if (message.Type == CustomWebSocketMessageType.Status && message.Status == "config_update")
        {
            _logger.Information("Received config update: {Data}", message.Message);

            try
            {
                var updateData = JsonConvert.DeserializeObject<Dictionary<string, object>>(message.Message);
                if (updateData != null &&
                    updateData.ContainsKey("PropertyPath") &&
                    updateData.ContainsKey("Value"))
                {
                    var propertyPath = updateData["PropertyPath"]?.ToString();
                    var newValue = updateData["Value"];
                    _configurationService.UpdateConfigFromExternal(propertyPath, newValue);
                }
                else
                {
                    _logger.Warning("Missing 'PropertyPath' or 'Value' in config update message.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling config update message.");
            }
        }
        else
        {
            _logger.Warning(
                "Unhandled message on /config endpoint: Type={Type}, Status={Status}",
                message.Type,
                message.Status
            );
        }
    }

    private async Task HandleGeneralMessageAsync(WebSocketMessage message, string endpoint)
    {
        if (message.Type == CustomWebSocketMessageType.Status ||
            message.Type == CustomWebSocketMessageType.Progress)
        {
            if (message.Progress > 0)
            {
                await _notificationService.UpdateProgress(
                    message.TaskId,
                    message.Message,
                    message.Progress
                );
            }
            else
            {
                await _notificationService.ShowNotification(message.Message, SoundType.GeneralChime);
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