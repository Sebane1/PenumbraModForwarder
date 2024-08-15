using System.Collections.Concurrent;

namespace FFXIVModExractor.Models;

public class ProcessingQueue
{
    public ConcurrentQueue<ProcessingQueueItem> Files { get; } = new();
}