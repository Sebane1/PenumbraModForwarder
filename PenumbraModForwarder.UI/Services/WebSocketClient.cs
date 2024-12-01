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

public class WebSocketClient : IWebSocketClient
{
    private readonly Dictionary<string, ClientWebSocket> _webSockets;
    private readonly INotificationService _notificationService;
    private readonly CancellationTokenSource _cts = new();
    private readonly string[] _endpoints = { "/status", "/currentTask" };
    private bool _isReconnecting;

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
                foreach (var endpoint in _endpoints)
                {
                    if (!_webSockets.ContainsKey(endpoint))
                    {
                        _webSockets[endpoint] = new ClientWebSocket();
                    }

                    var webSocket = _webSockets[endpoint];
                    if (webSocket.State != WebSocketState.Open)
                    {
                        await webSocket.ConnectAsync(new Uri($"ws://localhost:5000{endpoint}"), _cts.Token);
                        _ = ReceiveMessagesAsync(webSocket, endpoint);
                    }
                }

                _isReconnecting = false;
                return;
            }
            catch (Exception ex)
            {
                if (!_isReconnecting)
                {
                    Log.Error(ex, "WebSocket connection failed. Attempting to reconnect...");
                    await _notificationService.ShowNotification("Connection to background service lost. Reconnecting...");
                    _isReconnecting = true;
                }
                await Task.Delay(5000, _cts.Token);
            }
        }
    }

    private async Task ReceiveMessagesAsync(ClientWebSocket webSocket, string endpoint)
    {
        try
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = JsonConvert.DeserializeObject<WebSocketMessage>(
                        Encoding.UTF8.GetString(buffer, 0, result.Count));
                    
                    Log.Information("Received message from {Endpoint}: {Message}", endpoint, message);

                    if (message.Type == CustomWebSocketMessageType.Status)
                    {
                        await _notificationService.ShowNotification(message.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error receiving WebSocket messages from {Endpoint}", endpoint);
            if (!_isReconnecting)
            {
                await _notificationService.ShowNotification("Connection to background service lost. Reconnecting...");
            }
        }
    }
}