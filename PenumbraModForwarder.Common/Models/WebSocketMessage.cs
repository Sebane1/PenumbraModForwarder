using Newtonsoft.Json;

namespace PenumbraModForwarder.Common.Models;

public static class WebSocketMessageType
{
    public const string Progress = "progress_update";
    public const string Error = "error";
    public const string Status = "status";
}

public static class WebSocketMessageStatus
{
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Queued = "queued";
}

public class WebSocketMessage
{
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("task_id")]
    public string TaskId { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; }
    
    [JsonProperty("progress")]
    public int Progress { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }

    public static WebSocketMessage CreateProgress(string taskId, int progress, string message)
    {
        return new WebSocketMessage
        {
            Type = WebSocketMessageType.Progress,
            TaskId = taskId,
            Status = WebSocketMessageStatus.InProgress,
            Progress = progress,
            Message = message
        };
    }

    public static WebSocketMessage CreateError(string taskId, string errorMessage)
    {
        return new WebSocketMessage
        {
            Type = WebSocketMessageType.Error,
            TaskId = taskId,
            Status = WebSocketMessageStatus.Failed,
            Progress = 0,
            Message = errorMessage
        };
    }

    public static WebSocketMessage CreateStatus(string taskId, string status, string message)
    {
        return new WebSocketMessage
        {
            Type = WebSocketMessageType.Status,
            TaskId = taskId,
            Status = status,
            Progress = 0,
            Message = message
        };
    }
}