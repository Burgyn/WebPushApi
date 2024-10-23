using System.Collections.Concurrent;
using WebPush;

namespace WebPushApi;

public interface ISubscriptionProvider
{
    IEnumerable<PushSubscription> GetSubscriptions();

    Task AddSubscription(PushSubscription subscription);

    Task<PushSubscription> GetSubscriptionsAsync(string endpoint);
}

public class SubscriptionProvider : ISubscriptionProvider
{
    private readonly ConcurrentDictionary<string, PushSubscription> _subscription = [];

    public Task AddSubscription(PushSubscription subscription)
    {
        _subscription[subscription.Endpoint] = subscription;

        return Task.CompletedTask;
    }

    public IEnumerable<PushSubscription> GetSubscriptions()
        => _subscription.Values;

    public Task<PushSubscription> GetSubscriptionsAsync(string endpoint)
        => Task.FromResult(_subscription[endpoint]);
}