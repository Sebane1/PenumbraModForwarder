using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class WebSocketServer : IWebSocketServer, IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, ConnectionInfo>> _endpoints;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly HttpListener _httpListener;
    private Task _listenerTask;
    private bool _isStarted;

    public WebSocketServer()
    {
        _endpoints = new ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, ConnectionInfo>>();
        _cancellationTokenSource = new CancellationTokenSource();
        _httpListener = new HttpListener();
    }

    public void Start()
    {
        if (_isStarted) return;

        try
        {
            _httpListener.Prefixes.Add("http://localhost:5000/");
            _httpListener.Start();
            _isStarted = true;
            _listenerTask = StartListenerAsync();
            Log.Information("WebSocket server started successfully");
        }
        catch (HttpListenerException ex)
        {
            Log.Error(ex, "Failed to start WebSocket server");
            throw;
        }
    }
    
    public async Task UpdateCurrentTaskStatus(string status)
    {
        var message = new WebSocketMessage
        {
            Type = "status",
            Status = WebSocketMessageStatus.InProgress,
            Message = status
        };

        await BroadcastToEndpointAsync("/currentTask", message);
    }
    
    // This might not be needed, could cover everything inside UpdateCurrentTaskStatus
    public async Task UpdateConversionProgress(int progress, string status)
    {
        var message = new WebSocketMessage
        {
            Type = "progress",
            Status = WebSocketMessageStatus.InProgress,
            Progress = progress,
            Message = status
        };

        await BroadcastToEndpointAsync("/conversion", message);
    }

    private async Task StartListenerAsync()
    {
        while (_isStarted)
        {
            try
            {
                var context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleConnectionAsync(webSocketContext.WebSocket, context.Request.RawUrl);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in WebSocket listener");
            }
        }
    }

    public async Task HandleConnectionAsync(WebSocket webSocket, string endpoint)
    {
        var connections = _endpoints.GetOrAdd(endpoint, _ => new ConcurrentDictionary<WebSocket, ConnectionInfo>());
        var connectionInfo = new ConnectionInfo { LastPing = DateTime.UtcNow };
        connections.TryAdd(webSocket, connectionInfo);

        try
        {
            await MaintainConnectionAsync(webSocket, endpoint);
        }
        catch (WebSocketException ex)
        {
            Log.Error(ex, "WebSocket error for endpoint {Endpoint}", endpoint);
        }
        finally
        {
            await RemoveConnectionAsync(webSocket, endpoint);
        }
    }

    private async Task MaintainConnectionAsync(WebSocket webSocket, string endpoint)
    {
        var buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                _cancellationTokenSource.Token
            );

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    _cancellationTokenSource.Token
                );
            }
        }
    }

    public async Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message)
    {
        if (!_endpoints.TryGetValue(endpoint, out var connections)) return;

        var json = JsonConvert.SerializeObject(message);
        var bytes = Encoding.UTF8.GetBytes(json);

        var deadSockets = new List<WebSocket>();

        foreach (var (socket, _) in connections)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        _cancellationTokenSource.Token
                    );
                }
                else
                {
                    deadSockets.Add(socket);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error broadcasting to client");
                deadSockets.Add(socket);
            }
        }

        foreach (var socket in deadSockets)
        {
            await RemoveConnectionAsync(socket, endpoint);
        }
    }

    private async Task RemoveConnectionAsync(WebSocket socket, string endpoint)
    {
        if (_endpoints.TryGetValue(endpoint, out var connections))
        {
            connections.TryRemove(socket, out _);
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, _cancellationTokenSource.Token);
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _httpListener.Stop();
        _isStarted = false;
    }
}

public class ConnectionInfo
{
    public DateTime LastPing { get; set; }
}