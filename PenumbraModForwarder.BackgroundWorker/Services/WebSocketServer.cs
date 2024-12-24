using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.BackgroundWorker.Events;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;
using CustomWebSocketMessageType = PenumbraModForwarder.Common.Models.WebSocketMessageType;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class WebSocketServer : IWebSocketServer, IDisposable
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, ConnectionInfo>> _endpoints;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly HttpListener _httpListener;
    private Task _listenerTask;
    private bool _isStarted;
    private int _port;
    
    public event EventHandler<WebSocketMessageEventArgs> MessageReceived;

    public WebSocketServer(IConfigurationService configurationService)
    {
        _logger = Log.ForContext<WebSocketServer>();
        _configurationService = configurationService;
        _endpoints = new ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, ConnectionInfo>>();
        _cancellationTokenSource = new CancellationTokenSource();
        _httpListener = new HttpListener();
    }

    public void Start(int port)
    {
        if (_isStarted) return;
        _port = port;
        try
        {
            _httpListener.Prefixes.Add($"http://localhost:{_port}/");
            _httpListener.Start();
            _isStarted = true;
            _logger.Information("WebSocket server started successfully on port {Port}", _port);
            _listenerTask = StartListenerAsync();
        }
        catch (HttpListenerException ex)
        {
            _logger.Error(ex, "Failed to start WebSocket server");
            throw;
        }
    }

    private async Task StartListenerAsync()
    {
        try
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
                catch (HttpListenerException) when (_cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in WebSocket listener");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in StartListenerAsync");
        }
    }

    public async Task HandleConnectionAsync(WebSocket webSocket, string endpoint)
    {
        var connections = _endpoints.GetOrAdd(endpoint, _ => new ConcurrentDictionary<WebSocket, ConnectionInfo>());
        var connectionInfo = new ConnectionInfo { LastPing = DateTime.UtcNow };
        connections.TryAdd(webSocket, connectionInfo);
        _logger.Information("Client connected to endpoint {Endpoint}", endpoint);
        try
        {
            await ReceiveMessagesAsync(webSocket, endpoint);
        }
        catch (WebSocketException ex) when (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _logger.Error(ex, "WebSocket error for endpoint {Endpoint}", endpoint);
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("WebSocket connection closed during shutdown for endpoint {Endpoint}", endpoint);
        }
        finally
        {
            await RemoveConnectionAsync(webSocket, endpoint);
        }
    }

    private async Task ReceiveMessagesAsync(WebSocket webSocket, string endpoint)
    {
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error receiving WebSocket messages from {Endpoint}", endpoint);
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonConvert.DeserializeObject<WebSocketMessage>(messageJson);
                _logger.Information("Received message from {Endpoint}: {Message}", endpoint, messageJson);

                // Raise the MessageReceived event
                MessageReceived?.Invoke(this, new WebSocketMessageEventArgs
                {
                    Endpoint = endpoint,
                    Message = message
                });
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await CloseWebSocketAsync(webSocket);
                break;
            }
        }
    }

    private async Task CloseWebSocketAsync(WebSocket webSocket)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Error during WebSocket closure");
            }
        }
    }

    public async Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message)
    {
        if (!_endpoints.TryGetValue(endpoint, out var connections) || !connections.Any())
        {
            _logger.Debug("No clients connected to endpoint {Endpoint}, message queued", endpoint);
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
                    _logger.Information("Sending message to endpoint {Endpoint}: {Message}", endpoint, json);
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                }
                else
                {
                    deadSockets.Add(socket);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error broadcasting to client");
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
            _isStarted = false;
            _cancellationTokenSource.Cancel();
            // Close all active connections
            var closeTasks = _endpoints
                .SelectMany(endpoint => endpoint.Value.Select(async connection => await CloseWebSocketAsync(connection.Key)))
                .ToList();
            Task.WhenAll(closeTasks).Wait(TimeSpan.FromSeconds(5));
            _httpListener.Close();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during WebSocket server disposal");
        }
    }
}

public class ConnectionInfo
{
    public DateTime LastPing { get; set; }
}
