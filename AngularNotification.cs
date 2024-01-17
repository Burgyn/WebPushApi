namespace WebPushApi;

public class AngularNotification
{
    public class NotificationAction
    {
        public string Action { get; set; } = string.Empty;
        public string Title { get; } = string.Empty;
    }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public IList<int> Vibrate { get; set; } = new  List<int>();
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    public IList<NotificationAction> Actions { get; set; } = new  List<NotificationAction>();
}