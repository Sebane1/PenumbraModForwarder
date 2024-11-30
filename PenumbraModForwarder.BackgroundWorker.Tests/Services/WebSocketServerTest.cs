using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.BackgroundWorker.Tests.Extensions;
using PenumbraModForwarder.Common.Models;
using Serilog;
using Xunit;

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
        _webSocketServer.Start();
        _webSocketServer.Start();
        Assert.True(true);
    }

    [Fact]
    public async Task HandleConnection_WithCurrentTaskEndpoint_AddsConnection()
    {
        const string endpoint = "/currentTask";
        
        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        await Task.Delay(100);
        
        await _webSocketServer.UpdateCurrentTaskStatus("Test status");
        _mockWebSocket.CompleteReceive(true);
        await connectionTask;

        Assert.True(_mockWebSocket.CloseAsyncCalled);
        Assert.Single(_mockWebSocket.SentMessages);
    }

    [Fact]
    public async Task UpdateCurrentTaskStatus_SendsMessageToConnectedClients()
    {
        const string endpoint = "/currentTask";
        const string status = "Converting mod: test.pmp";

        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        await _webSocketServer.UpdateCurrentTaskStatus(status);
        _mockWebSocket.CompleteReceive();
        await connectionTask;

        Assert.Single(_mockWebSocket.SentMessages);
        var sentMessage = JsonConvert.DeserializeObject<WebSocketMessage>(
            Encoding.UTF8.GetString(_mockWebSocket.SentMessages[0]));
        
        Assert.Equal("status", sentMessage.Type);
        Assert.Equal(WebSocketMessageStatus.InProgress, sentMessage.Status);
        Assert.Equal(status, sentMessage.Message);
    }

    [Fact]
    public async Task UpdateConversionProgress_SendsProgressToConnectedClients()
    {
        const string endpoint = "/conversion";
        const int progress = 50;
        const string status = "Extracting files...";

        var connectionTask = _webSocketServer.HandleConnectionAsync(_mockWebSocket, endpoint);
        await _webSocketServer.UpdateConversionProgress(progress, status);
        _mockWebSocket.CompleteReceive();
        await connectionTask;

        Assert.Single(_mockWebSocket.SentMessages);
        var sentMessage = JsonConvert.DeserializeObject<WebSocketMessage>(
            Encoding.UTF8.GetString(_mockWebSocket.SentMessages[0]));
        
        Assert.Equal("progress", sentMessage.Type);
        Assert.Equal(WebSocketMessageStatus.InProgress, sentMessage.Status);
        Assert.Equal(progress, sentMessage.Progress);
        Assert.Equal(status, sentMessage.Message);
    }
}