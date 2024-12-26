using Newtonsoft.Json;

namespace PenumbraModForwarder.Common.Models;

public class PenumbraModPath
{
    [JsonProperty("ModDirectory")]
    public string ModDirectory { get; set; }
}