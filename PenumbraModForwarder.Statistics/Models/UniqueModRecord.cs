using LiteDB;

namespace PenumbraModForwarder.Statistics.Models;

public class UniqueModRecord
{
    public ObjectId Id { get; set; }
    public string ModName { get; set; }
}