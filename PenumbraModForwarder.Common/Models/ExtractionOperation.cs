namespace PenumbraModForwarder.Common.Models;

public class ExtractionOperation
{
    public string FilePath { get; }
    
    public ExtractionOperation(string filePath)
    {
        FilePath = filePath;
    }
}