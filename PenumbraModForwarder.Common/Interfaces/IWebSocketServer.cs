using System.Net.WebSockets;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IWebSocketServer
{
    void Start();
    Task HandleConnectionAsync(WebSocket webSocket, string endpoint);
    Task BroadcastToEndpointAsync(string endpoint, WebSocketMessage message);
    Task UpdateCurrentTaskStatus(string status);
}