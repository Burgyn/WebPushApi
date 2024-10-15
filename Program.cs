using System.Text.Json;
using WebPush;
using WebPushApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISubscriptionProvider, SubscriptionProvider>();
builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.MapPost("/pushNotification", async (NotificationMessage message, IConfiguration configuration, ISubscriptionProvider provider, ILoggerFactory loggerFactory) =>
{
    string subject = configuration["Vapid:Subject"]!;
    string publicKey = configuration["Vapid:PublicKey"]!;
    string privateKey = configuration["Vapid:PrivateKey"]!;
    var options = new Dictionary<string, object>
    {
        ["vapidDetails"] = new VapidDetails(subject, publicKey, privateKey)
    };

    var webPushClient = new WebPushClient();

    var angularNotificationString = JsonSerializer.Serialize(new AngularNotification()
    {
        Notification = new()
        {
            Body = message.Body,
            Title = message.Title,
            Icon = message.Icon
        }
    }, options: new () { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    await foreach (PushSubscription sub in provider.GetSubscriptions())
    {
        try
        {
            await webPushClient.SendNotificationAsync(sub, angularNotificationString, options);
        }
        catch (WebPushException exception)
        {
            loggerFactory.CreateLogger("notification").LogError(exception, "Error sending push notification");
        }
    }
});

app.Run();
