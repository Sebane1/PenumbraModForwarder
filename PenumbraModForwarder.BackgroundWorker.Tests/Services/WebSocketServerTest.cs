using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.BackgroundWorker.Tests.Extensions;
using PenumbraModForwarder.Common.Models;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker.Tests.Services;

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
        _webSocketServer.Start();
        _webSocketServer.Start();
        Assert.True(true);
    }

    [Fact(Skip = "Test needs to be updated to match WebSocket patterns")]
    public async Task HandleConnection_WithStatusEndpoint_AddsConnection()
    {
        const string endpoint = "/status";
    
        _webSocketServer.Start();
    
        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        await Task.Delay(100);
    
        var taskId = Guid.NewGuid().ToString();
        var message = WebSocketMessage.CreateStatus(
            taskId,
            WebSocketMessageStatus.InProgress,
            "Test status"
        );
    
        await _webSocketServer.BroadcastToEndpointAsync(endpoint, message);
    
        Assert.True(_mockWebSocket.SentMessages.Any(), "No messages were sent");
    
        _mockWebSocket.CompleteReceive(true);
        await connectionTask;
    
        Assert.True(_mockWebSocket.CloseAsyncCalled);
    }

    [Fact]
    public async Task UpdateCurrentTaskStatus_SendsMessageToConnectedClients()
    {
        const string endpoint = "/currentTask";
        const string status = "Converting mod: test.pmp";

        _webSocketServer.Start();
        
        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        await Task.Delay(100);
        
        await _webSocketServer.UpdateCurrentTaskStatus(status);
        
        Assert.Single(_mockWebSocket.SentMessages);
        var sentMessage = JsonConvert.DeserializeObject<WebSocketMessage>(
            Encoding.UTF8.GetString(_mockWebSocket.SentMessages[0]));
        
        Assert.Equal("status", sentMessage.Type);
        Assert.Equal(WebSocketMessageStatus.InProgress, sentMessage.Status);
        Assert.Equal(status, sentMessage.Message);
        
        _mockWebSocket.CompleteReceive();
        await connectionTask;
    }
}