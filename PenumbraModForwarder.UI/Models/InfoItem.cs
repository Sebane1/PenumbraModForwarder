using System;

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

    public override bool Equals(object? obj)
    {
        if (obj is InfoItem other)
        {
            return Name == other.Name && Value == other.Value;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Value);
    }
}