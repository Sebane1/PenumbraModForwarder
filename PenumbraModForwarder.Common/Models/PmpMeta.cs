namespace PenumbraModForwarder.Common.Models;

public class PmpMeta
{
    public int FileVersion { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public string Version { get; set; }
    public string Website { get; set; }
    public List<string> ModTags { get; set; }
}