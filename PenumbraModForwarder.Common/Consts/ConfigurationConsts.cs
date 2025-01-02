namespace PenumbraModForwarder.Common.Consts;

public static class ConfigurationConsts
{
    /// <summary>
    /// This is where everything will go, inside configuration files, extracted files, queue saves
    /// </summary>
    public static readonly string ConfigurationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder";
    
    /// <summary>
    /// This is where configuration options will be stored
    /// Example:
    ///     Download Path - Where downloads will be found
    /// </summary>
    public static readonly string ConfigurationFilePath = ConfigurationPath + @"\config-v3.json"; // Change this to @"\config.json" when ready for prod
    
    /// <summary>
    /// The folder location where mods will be moved to after found inside the download folder
    /// This is so we can do nice cleanups and users will have a spot to find all mods that have been downloaded
    /// Maybe have a history.json as well?
    /// </summary>
    public static readonly string ModsPath = ConfigurationPath + @"\mods\";
    
    /// <summary>
    /// The folder location where logs will be found
    /// </summary>
    public static readonly string LogsPath = ConfigurationPath + @"\logs\";
    
    /// <summary>
    /// The user should never be able to set what this is
    /// This is where we cache the mods for the homeview
    /// </summary>
    internal static readonly string CachePath = ConfigurationPath + @"\cache\";
}