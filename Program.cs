using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using WebPush;
using WebPushApi;

//dummy

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<VapidOptions>().Bind(builder.Configuration.GetSection("Vapid")).ValidateDataAnnotations();

builder.Services.AddSingleton<ISubscriptionProvider, SubscriptionProvider>();
builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(b => b
   .AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader()
);

app.MapGet("/subscriptions", (ISubscriptionProvider provider)
    => provider.GetSubscriptions());

app.MapGet("/vapidKey", (IConfiguration config) => new VapidKey()
{
    PublicKey = config["Vapid:PublicKey"] ?? string.Empty
});

app.MapPost("/subscriptions", async (BrowserPushSubscription subscription, ISubscriptionProvider provider) =>
{
    await provider.AddSubscription(new PushSubscription()
    {
        Auth = subscription.Keys.Auth,
        P256DH = subscription.Keys.P256DH,
        Endpoint = subscription.Endpoint
    });
});

app.MapPost("/pushNotification", async (NotificationMessage message, ISubscriptionProvider provider, ILoggerFactory loggerFactory, IOptions<VapidOptions> vapid) =>
{
    await OnHandleNotification(message, provider, loggerFactory, vapid, async (webPushClient, angularNotificationString, options) =>
    {
        await foreach (PushSubscription sub in provider.GetSubscriptions())
        {
            await webPushClient.SendNotificationAsync(sub, angularNotificationString, options);
        }
    });
});

app.MapPost("/pushNotification/{endpoint}", async (string endpoint, NotificationMessage message, ISubscriptionProvider provider, ILoggerFactory loggerFactory, IOptions<VapidOptions> vapid) =>
{
    endpoint = WebUtility.UrlDecode(endpoint);
    await OnHandleNotification(message, provider, loggerFactory, vapid, async (webPushClient, angularNotificationString, options) =>
    {
        var sub = await provider.GetSubscriptionsAsync(endpoint);
        await webPushClient.SendNotificationAsync(sub, angularNotificationString, options);
    });
});

app.Run();

static async Task OnHandleNotification(
    NotificationMessage message,
    ISubscriptionProvider provider,
    ILoggerFactory loggerFactory,
    IOptions<VapidOptions> vapid,
    Func<WebPushClient, string, Dictionary<string, object>,
    Task> func)
{
    var vapidOptions = vapid.Value;
    var options = new Dictionary<string, object>
    {
        ["vapidDetails"] = new VapidDetails(vapidOptions.Subject, vapidOptions.PublicKey, vapidOptions.PrivateKey)
    };

    var angularNotificationString = JsonSerializer.Serialize(new AngularNotification()
    {
        Notification = new()
        {
            Body = message.Body,
            Title = message.Title,
            Icon = message.Icon
        }
    }, options: new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    var webPushClient = new WebPushClient();
    try
    {
        await func(webPushClient, angularNotificationString, options);
    }
    catch (WebPushException exception)
    {
        loggerFactory.CreateLogger("notification").LogError(exception, "Error sending push notification");
    }
}

public class VapidOptions
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}