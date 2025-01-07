using Newtonsoft.Json;

namespace PenumbraModForwarder.Common.Extensions;

public static class HttpContextExtensions
{
    public static async Task<T?> ReadAsJsonAsync<T>(this HttpContent content)
    {
        var body = await content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
            return default;

        return JsonConvert.DeserializeObject<T>(body);
    }
}