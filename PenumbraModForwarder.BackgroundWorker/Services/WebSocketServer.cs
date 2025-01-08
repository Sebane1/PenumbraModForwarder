using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using NLog;
using PenumbraModForwarder.BackgroundWorker.Events;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using CustomWebSocketMessageType = PenumbraModForwarder.Common.Models.WebSocketMessageType;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class WebSocketServer : IWebSocketServer, IDisposable
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigurationService _configurationService;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, ConnectionInfo>> _endpoints;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly HttpListener _httpListener;
    private Task _listenerTask;
    private bool _isStarted;
    private int _port;
    private readonly string _serverId;

    public event EventHandler<WebSocketMessageEventArgs> MessageReceived;

    public WebSocketServer(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _endpoints = new ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, ConnectionInfo>>();
        _cancellationTokenSource = new CancellationTokenSource();
        _httpListener = new HttpListener();

        // Assign a unique ID to distinguish messages sent by this WebSocketServer
        _serverId = Guid.NewGuid().ToString("N");

        // Listen for configuration changes
        _configurationService.ConfigurationChanged += OnConfigurationChanged;
    }

    public void Start(int port)
    {
        if (_isStarted)
            return;

        _port = port;
        try
        {
            _httpListener.Prefixes.Add($"http://localhost:{_port}/");
            _httpListener.Start();
            _isStarted = true;

            _logger.Info("WebSocket server started successfully on port {Port}", _port);

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
        var connectionInfo = new ConnectionInfo
        {
            LastPing = DateTime.UtcNow
        };
        connections.TryAdd(webSocket, connectionInfo);

        _logger.Info("Client connected to endpoint {Endpoint}", endpoint);

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
            catch (WebSocketException ex) when (ex.InnerException is HttpListenerException { ErrorCode: 995 })
            {
                _logger.Debug("WebSocket aborted by HttpListener (995) for endpoint {Endpoint}", endpoint);
                break;
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
                _logger.Info("Received message from {Endpoint}: {MessageJson}", endpoint, messageJson);

                var message = JsonConvert.DeserializeObject<WebSocketMessage>(messageJson);
                if (message == null)
                {
                    _logger.Warn("Unable to deserialize WebSocketMessage from {Endpoint}", endpoint);
                    continue;
                }

                // If this message was sent by the server itself, ignore it
                if (message.ClientId == _serverId)
                {
                    _logger.Debug("Ignoring message from this server: {MessageJson}", messageJson);
                    continue;
                }

                if (message.Type?.Equals("configuration_change", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _logger.Debug("Handling message type 'configuration_change' from {Endpoint}", endpoint);
                    HandleConfigurationChange(message);
                }
                else if (message.Type == CustomWebSocketMessageType.Status && message.Status == "config_update")
                {
                    HandleConfigUpdateMessage(message);
                }
                else
                {
                    MessageReceived?.Invoke(this, new WebSocketMessageEventArgs
                    {
                        Endpoint = endpoint,
                        Message = message
                    });
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await CloseWebSocketAsync(webSocket);
                break;
            }
        }
    }

    private void HandleConfigurationChange(WebSocketMessage message)
    {
        _logger.Debug("Message indicates a configuration change: {Message}", message.Message);
        try
        {
            var updateData = JsonConvert.DeserializeObject<Dictionary<string, object>>(message.Message);
            if (updateData == null)
            {
                _logger.Warn("configuration_change message had invalid or non-JSON payload.");
                return;
            }

            if (!updateData.ContainsKey("PropertyPath") || !updateData.ContainsKey("NewValue"))
            {
                _logger.Warn("configuration_change payload missing 'PropertyPath' or 'NewValue'.");
                return;
            }

            var propertyPath = updateData["PropertyPath"]?.ToString();
            var newValue = updateData["NewValue"];

            _logger.Debug("Updating config property '{PropertyPath}' to new value: {NewValue}", propertyPath, newValue);
            _configurationService.UpdateConfigFromExternal(propertyPath, newValue);
            _logger.Info("Configuration updated for '{PropertyPath}'", propertyPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling configuration_change message");
        }
    }

    private void HandleConfigUpdateMessage(WebSocketMessage message)
    {
        _logger.Info("Processing config update request: {Data}", message.Message);
        try
        {
            var updateData = JsonConvert.DeserializeObject<Dictionary<string, object>>(message.Message);
            if (updateData != null &&
                updateData.ContainsKey("PropertyPath") &&
                updateData.ContainsKey("Value"))
            {
                var propertyPath = updateData["PropertyPath"].ToString();
                var newValue = updateData["Value"];

                _logger.Debug("Updating config property at path {Path} to new value: {Value}", propertyPath, newValue);
                _configurationService.UpdateConfigFromExternal(propertyPath, newValue);

                _logger.Debug("Config update completed for property path {Path}", propertyPath);
            }
            else
            {
                _logger.Warn("Unable to process config update, required keys not found in message data.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling config update message.");
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
                _logger.Debug(ex, "Error during WebSocket closure");
            }
        }
    }

    public async Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message)
    {
        message.ClientId = _serverId;

        if (!_endpoints.TryGetValue(endpoint, out var connections) || !connections.Any())
        {
            _logger.Debug("No clients connected to endpoint {Endpoint}, message not sent", endpoint);
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
                    _logger.Info("Sending message to endpoint {Endpoint}: {Message}", endpoint, json);
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
            if (_httpListener.IsListening)
            {
                _httpListener.Stop();
            }

            _isStarted = false;
            _cancellationTokenSource.Cancel();

            var closeTasks = _endpoints
                .SelectMany(ep => ep.Value.Select(async connection => await CloseWebSocketAsync(connection.Key)))
                .ToList();

            Task.WhenAll(closeTasks).Wait(TimeSpan.FromSeconds(5));
            _httpListener.Close();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during WebSocket server disposal");
        }
    }

    private async void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
    {
        try
        {
            _logger.Debug(
                "OnConfigurationChanged triggered for property {PropertyName} with new value: {NewValue}",
                e.PropertyName,
                e.NewValue
            );

            var updateData = new Dictionary<string, object>
            {
                { "PropertyPath", e.PropertyName },
                { "Value", e.NewValue }
            };

            var message = new WebSocketMessage
            {
                Type = CustomWebSocketMessageType.Status,
                Status = "config_update",
                Message = JsonConvert.SerializeObject(updateData)
            };

            _logger.Debug("Broadcasting config change to /config endpoint: {Payload}", message.Message);
            await BroadcastToEndpointAsync("/config", message);
            _logger.Debug("Config change broadcast completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error broadcasting config change");
        }
    }
}

public class ConnectionInfo
{
    public DateTime LastPing { get; set; }
}