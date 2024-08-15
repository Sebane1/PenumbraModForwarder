namespace PenumbraModForwarder.Services;

// TODO: This is unneeded, will be used in refactor.
public static class TexToolsFinder
{
    private static string _textoolsPath;

    public static void FindTexToolsPath()
    {
        var textoolsInk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Microsoft\Windows\Start Menu\Programs\FFXIV TexTools\FFXIV TexTools.lnk");
        if (File.Exists(textoolsInk))
        {
            var texToolsDirectory = Path.GetDirectoryName(ShortcutHandler.GetShortcutTarget(textoolsInk));
            _textoolsPath = Path.Combine(texToolsDirectory, "ConsoleTools.exe");
        }
    }

    public static string GetTexToolsPath()
    {
        return _textoolsPath;
    }
}