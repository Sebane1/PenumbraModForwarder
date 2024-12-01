using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
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
        while (_isStarted && !_cancellationTokenSource.Token.IsCancellationRequested)
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
            catch (Exception ex) when (!_cancellationTokenSource.Token.IsCancellationRequested)
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

        Log.Information("Client connected to endpoint {Endpoint}", endpoint);

        try
        {
            await MaintainConnectionAsync(webSocket, endpoint);
        }
        catch (WebSocketException ex) when (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            Log.Error(ex, "WebSocket error for endpoint {Endpoint}", endpoint);
        }
        catch (OperationCanceledException)
        {
            Log.Debug("WebSocket connection closed during shutdown for endpoint {Endpoint}", endpoint);
        }
        finally
        {
            await RemoveConnectionAsync(webSocket, endpoint);
        }
    }

    private async Task MaintainConnectionAsync(WebSocket webSocket, string endpoint)
    {
        var buffer = new byte[1024];
    
        try
        {
            while (webSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cancellationTokenSource.Token
                );

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseWebSocketAsync(webSocket);
                    break;
                }
            }
        }
        catch (WebSocketException ex) when (ex.InnerException is HttpListenerException listenerEx && 
                                            (listenerEx.ErrorCode == 995 || _cancellationTokenSource.Token.IsCancellationRequested))
        {
            Log.Debug("WebSocket connection closed during shutdown for endpoint {Endpoint}", endpoint);
            await CloseWebSocketAsync(webSocket);
        }
        catch (OperationCanceledException)
        {
            Log.Debug("WebSocket connection cancelled for endpoint {Endpoint}", endpoint);
            await CloseWebSocketAsync(webSocket);
        }
    }

    private async Task CloseWebSocketAsync(WebSocket webSocket)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Server shutting down",
                    CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error during WebSocket closure");
            }
        }
    }

    public async Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message)
    {
        if (!_endpoints.TryGetValue(endpoint, out var connections) || !connections.Any())
        {
            Log.Debug("No clients connected to endpoint {Endpoint}, message queued", endpoint);
            return;
        }

        var json = JsonConvert.SerializeObject(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        var deadSockets = new List<WebSocket>();

        foreach (var (socket, _) in connections)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    Log.Information("Sending message to endpoint {Endpoint}: {Message}", endpoint, json);
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

    public bool HasConnectedClients()
    {
        return _endpoints.Any(e => e.Value.Any());
    }

    private async Task RemoveConnectionAsync(WebSocket socket, string endpoint)
    {
        if (_endpoints.TryGetValue(endpoint, out var connections))
        {
            connections.TryRemove(socket, out _);
            await CloseWebSocketAsync(socket);
        }
    }

    public void Dispose()
    {
        try
        {
            _cancellationTokenSource.Cancel();
        
            // Close all active connections first
            var closeTasks = _endpoints
                .SelectMany(endpoint => endpoint.Value.Select(async connection => 
                    await CloseWebSocketAsync(connection.Key)))
                .ToList();
        
            Task.WhenAll(closeTasks).Wait(TimeSpan.FromSeconds(5));
        
            _httpListener.Stop();
            _isStarted = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during WebSocket server disposal");
        }
    }
}

public class ConnectionInfo
{
    public DateTime LastPing { get; set; }
}