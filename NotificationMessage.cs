using static WebPushApi.AngularNotification.NotificationWrapper;

namespace WebPushApi;

public class NotificationMessage
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    public IList<NotificationAction> Actions { get; set; } = [];
}