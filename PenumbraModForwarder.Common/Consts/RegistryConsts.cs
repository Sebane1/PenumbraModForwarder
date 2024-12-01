namespace PenumbraModForwarder.Common.Consts;

public static class RegistryConsts
{
    /// <summary>
    /// Add PMF to Open With menus for Mods
    /// </summary>
    public static string HKLMOpenCommandPath = @"SOFTWARE\Classes\PenumbraModpackFile\shell\open\command";
    /// <summary>
    /// Add PMF to Open With menus for Mods
    /// </summary>
    public static string HKCUClassesPath = @"Software\Classes\";
    /// <summary>
    /// Path of where TexTools is usually installed to
    /// </summary>
    public static string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";
}