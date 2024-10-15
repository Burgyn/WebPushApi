using System.Text.Json;
using WebPush;

namespace WebPushApi;

public static class Helpers
{
    public static async Task<List<PushSubscription>> PushSubscriptions(string filename)
    {
        if (!File.Exists(filename))
        {
            return [];
        }
        List<PushSubscription> pushSubscriptions = [];
        string data = await File.ReadAllTextAsync(filename);
        if (!string.IsNullOrEmpty(data))
        {
            pushSubscriptions = JsonSerializer.Deserialize<List<PushSubscription>>(data) ?? [];
        }

        return pushSubscriptions;
    }
}