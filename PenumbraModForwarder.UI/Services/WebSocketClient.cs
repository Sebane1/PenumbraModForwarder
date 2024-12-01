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
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;
using CustomWebSocketMessageType = PenumbraModForwarder.Common.Models.WebSocketMessageType;

namespace PenumbraModForwarder.UI.Services;

public class WebSocketClient : IWebSocketClient, IDisposable
{
    private readonly Dictionary<string, ClientWebSocket> _webSockets;
    private readonly INotificationService _notificationService;
    private readonly CancellationTokenSource _cts = new();
    private readonly string[] _endpoints = { "/status", "/currentTask" };
    private bool _isReconnecting;
    private int _retryCount = 0;

    public WebSocketClient(INotificationService notificationService)
    {
        _webSockets = new Dictionary<string, ClientWebSocket>();
        _notificationService = notificationService;
    }

    public async Task ConnectAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                await ConnectEndpointsAsync();
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
                Log.Error(ex, "Connection loop error. Retry attempt: {RetryCount}", _retryCount);
                await _notificationService.ShowNotification(
                    $"Connection failed. Retrying in 5 seconds... (Attempt {_retryCount})", 
                    5
                );
                await Task.Delay(5000, _cts.Token);
            }
        }
    }

    private async Task ConnectEndpointsAsync()
    {
        foreach (var endpoint in _endpoints)
        {
            try
            {
                if (!_webSockets.ContainsKey(endpoint) || 
                    _webSockets[endpoint].State != WebSocketState.Open)
                {
                    if (_webSockets.ContainsKey(endpoint))
                    {
                        await DisconnectWebSocketAsync(_webSockets[endpoint]);
                    }

                    var webSocket = new ClientWebSocket();
                    _webSockets[endpoint] = webSocket;

                    await webSocket.ConnectAsync(
                        new Uri($"ws://localhost:5000{endpoint}"), 
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
                    Log.Error(ex, "WebSocket connection failed for endpoint {Endpoint}. Attempting to reconnect...", endpoint);
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
            Log.Error(ex, "Error during WebSocket disconnect");
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
                    var message = JsonConvert.DeserializeObject<WebSocketMessage>(
                        Encoding.UTF8.GetString(buffer, 0, result.Count)
                    );

                    Log.Information("Received message from {Endpoint}: {Message}", endpoint, message);

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
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error receiving WebSocket messages from {Endpoint}", endpoint);
            if (!_isReconnecting)
            {
                await _notificationService.ShowNotification(
                    $"Lost connection to {endpoint}. Attempting to reconnect..."
                );
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