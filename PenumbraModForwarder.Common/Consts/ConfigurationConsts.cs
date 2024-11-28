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
    public static readonly string ConfigurationFilePath = ConfigurationPath + @"\config.json";
    
    /// <summary>
    /// The folder location for where extracted mods will go
    /// </summary>
    public static readonly string ExtractionPath = ConfigurationPath + @"\extraction\";
    
    /// <summary>
    /// The folder location where mods will be moved to after found inside the download folder
    /// This is so we can do nice cleanups and users will have a spot to find all mods that have been downloaded
    /// Maybe have a history.json as well?
    /// </summary>
    public static readonly string ModsPath = ConfigurationPath + @"\mods\";
    
    /// <summary>
    /// The folder location for where mods converted from EndWalker To DawnTrail will be located
    /// </summary>
    public static readonly string ConversionPath = ConfigurationPath + @"\conversion\";
    
    /// <summary>
    /// The folder location where logs will be found
    /// </summary>
    public static readonly string LogsPath = ConfigurationPath + @"\logs\";
}