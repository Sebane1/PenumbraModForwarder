using System.Reflection;

namespace SharedResources;

public static class ResourceLoader
{
    public static Stream? GetResourceStream(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        return resourceName == null ? null : assembly.GetManifestResourceStream(resourceName);
    }
}