using MessagePack;

namespace PenumbraModForwarder.Common.Models;

[MessagePackObject(AllowPrivate = true)]
internal class XmaCacheData
{
    [Key(0)]
    public List<XmaMods> Mods { get; set; } = new();

    [Key(1)]
    public DateTimeOffset ExpirationTime { get; set; }
}