using System.Net.WebSockets;
using PenumbraModForwarder.BackgroundWorker.Events;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.BackgroundWorker.Interfaces;

public interface IWebSocketServer
{
    void Start(int port);
    Task HandleConnectionAsync(WebSocket webSocket, string endpoint);
    Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message);
    bool HasConnectedClients();
    event EventHandler<WebSocketMessageEventArgs> MessageReceived;
}