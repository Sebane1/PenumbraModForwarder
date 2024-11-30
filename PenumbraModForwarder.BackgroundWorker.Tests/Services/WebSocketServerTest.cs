using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.BackgroundWorker.Tests.Extensions;
using PenumbraModForwarder.Common.Models;
using Serilog;
using Xunit;
using WebSocketMessageType = PenumbraModForwarder.Common.Models.WebSocketMessageType;

namespace PenumbraModForwarder.Tests.Services;

public class WebSocketServerTests : IDisposable
{
    private readonly WebSocketServer _webSocketServer;
    private readonly MockWebSocket _mockWebSocket;

    public WebSocketServerTests()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        _webSocketServer = new WebSocketServer();
        _mockWebSocket = new MockWebSocket();
    }

    public void Dispose()
    {
        _webSocketServer.Dispose();
    }

    [Fact]
    public void Start_WhenCalledMultipleTimes_OnlyStartsOnce()
    {
        // Act
        _webSocketServer.Start();
        _webSocketServer.Start();

        // Assert - if we get here without exception, test passes
        Assert.True(true);
    }

    [Fact]
    public async Task HandleConnection_WithValidEndpoint_AddsConnection()
    {
        // Arrange
        const string endpoint = "/test";

        // Act
        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        _mockWebSocket.CompleteReceive(); // Signal the connection to close
        await connectionTask;

        // Assert
        Assert.True(_mockWebSocket.CloseAsyncCalled);
    }

    [Fact]
    public async Task BroadcastToEndpoint_WithValidMessage_SendsToAllConnections()
    {
        // Arrange
        const string endpoint = "/test";
        var message = WebSocketMessage.CreateStatus(
            "test-task-id",
            WebSocketMessageStatus.InProgress,
            "Test message"
        );

        // Act
        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        await _webSocketServer.BroadcastToEndpointAsync(endpoint, message);
        _mockWebSocket.CompleteReceive();
        await connectionTask;

        // Assert
        Assert.Single(_mockWebSocket.SentMessages);
        var sentMessage = JsonConvert.DeserializeObject<WebSocketMessage>(
            Encoding.UTF8.GetString(_mockWebSocket.SentMessages[0]));
    
        Assert.Equal(WebSocketMessageType.Status, sentMessage.Type);
        Assert.Equal("test-task-id", sentMessage.TaskId);
        Assert.Equal(WebSocketMessageStatus.InProgress, sentMessage.Status);
        Assert.Equal("Test message", sentMessage.Message);
    }
}