using PenumbraModForwarder.Enums;

namespace FFXIVModExractor.Models;

public class ProcessingQueueItem
{
    public string FilePath { get; set; }
    public ProcessingStatus Status { get; set; }
}