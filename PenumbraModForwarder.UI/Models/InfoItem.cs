namespace PenumbraModForwarder.UI.Models;

public class InfoItem
{
    public string Name { get; set; }
    public string Value { get; set; }

    public InfoItem(string name, string value)
    {
        Name = name;
        Value = value;
    }
}