namespace PenumbraModForwarder.Common.Models;

public class GithubStaticResources
{
    public class InformationJson
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? SchemaVersion { get; set; }
        public string? UpdaterInfo { get; set; }
    }

    public class UpdaterInformationJson
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? SchemaVersion { get; set; }
        public BackgroundsInfo? Backgrounds { get; set; }
    }

    public class BackgroundsInfo
    {
        public string[]? Images { get; set; }
    }
}