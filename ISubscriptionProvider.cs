using System.Collections.Concurrent;
using WebPush;

namespace WebPushApi;

public interface ISubscriptionProvider
{
    IAsyncEnumerable<PushSubscription> GetSubscriptions();

    Task AddSubscription(PushSubscription subscription);
}

public class SubscriptionProvider : ISubscriptionProvider
{
    private readonly ConcurrentDictionary<string, PushSubscription> _subscription = [];

    public Task AddSubscription(PushSubscription subscription)
    {
        _subscription[subscription.Endpoint] = subscription;

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<PushSubscription> GetSubscriptions()
    {
        foreach (var item in _subscription.Values)
        {
            yield return item;
            await Task.Yield();
        }
    }
}