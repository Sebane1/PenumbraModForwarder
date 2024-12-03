using System.Net.WebSockets;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.BackgroundWorker.Interfaces;

public interface IWebSocketServer
{
    void Start(int port);
    Task HandleConnectionAsync(WebSocket webSocket, string endpoint);
    Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message);
    Task UpdateCurrentTaskStatus(string status);
    bool HasConnectedClients();
}