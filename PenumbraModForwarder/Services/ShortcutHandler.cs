namespace PenumbraModForwarder.Services;

public static class ShortcutHandler
{
    public static string GetShortcutTarget(string shortcutPath)
    {
        using (var fileStream = File.OpenRead(shortcutPath))
        using (var reader = new BinaryReader(fileStream))
        {
            fileStream.Seek(0x14, SeekOrigin.Begin);
            var flags = reader.ReadUInt32();
            if ((flags & 0x1) == 0)
                return null;

            fileStream.Seek(0x4C, SeekOrigin.Begin);
            fileStream.Seek(reader.ReadUInt16(), SeekOrigin.Current);
            var offset = fileStream.Position + reader.ReadUInt16();
            fileStream.Seek(offset, SeekOrigin.Begin);
            return ReadNullTerminatedString(reader);
        }
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var result = "";
        char ch;
        while ((ch = reader.ReadChar()) != 0)
            result += ch;
        return result;
    }
}