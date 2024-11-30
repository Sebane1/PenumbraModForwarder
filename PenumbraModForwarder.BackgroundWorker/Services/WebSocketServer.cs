using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Net;
using PenumbraModForwarder.Common.Models;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
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
        if (_isStarted)
        {
            return;
        }

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

    private async Task StartListenerAsync()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    var endpoint = context.Request.Url?.LocalPath ?? "/";
                    
                    _ = HandleConnectionAsync(webSocketContext.WebSocket, endpoint)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                                Log.Error(t.Exception, "WebSocket connection handler failed");
                        });
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        catch (Exception ex) when (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            Log.Error(ex, "WebSocket listener failed unexpectedly");
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
        var buffer = new byte[1024 * 4];
        
        while (webSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cancellationTokenSource.Token);
                    break;
                }

                if (_endpoints.TryGetValue(endpoint, out var connections) && 
                    connections.TryGetValue(webSocket, out var connectionInfo))
                {
                    connectionInfo.LastPing = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message)
    {
        if (!_endpoints.TryGetValue(endpoint, out var connections))
        {
            return;
        }

        var deadConnections = new List<WebSocket>();
        var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        var buffer = new ArraySegment<byte>(messageBytes);

        foreach (var (socket, _) in connections)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                }
                else
                {
                    deadConnections.Add(socket);
                }
            }
            catch (WebSocketException)
            {
                deadConnections.Add(socket);
            }
        }

        foreach (var deadSocket in deadConnections)
        {
            await RemoveConnectionAsync(deadSocket, endpoint);
        }
    }

    private async Task RemoveConnectionAsync(WebSocket webSocket, string endpoint)
    {
        if (_endpoints.TryGetValue(endpoint, out var connections))
        {
            connections.TryRemove(webSocket, out _);
            
            if (connections.IsEmpty)
            {
                _endpoints.TryRemove(endpoint, out _);
            }
        }

        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cancellationTokenSource.Token);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        if (_isStarted)
        {
            _httpListener.Stop();
            _httpListener.Close();
        }
        _cancellationTokenSource.Dispose();
    }

    private class ConnectionInfo
    {
        public DateTime LastPing { get; set; }
    }
}