using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebPush;
using WebPushApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
var app = builder.Build();
app.UseCors(b => b
   .AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader()
);

const string fileName = "subscriptions.json";
app.MapGet("/subscriptions", async () => await Helpers.PushSubscriptions(fileName));

app.MapGet("/vapidKey", (IConfiguration config) => new VapidKey(){PublicKey = config["Vapid:PublicKey"] ?? string.Empty});

app.MapPost("/subscriptions", async (BrowserPushSubscription subscription) =>
{
   var subscriptions = await Helpers.PushSubscriptions(fileName);
   subscriptions.Add(new PushSubscription()
   {
      Auth = subscription.Keys.Auth,
      P256DH = subscription.Keys.P256DH,
      Endpoint = subscription.Endpoint
   });
   
   File.WriteAllText(fileName,JsonSerializer.Serialize(subscriptions));
});

app.MapPost("/pushNotification", async (NotificationMessage message, IConfiguration configuration) =>
{
   var subscriptions = await Helpers.PushSubscriptions(fileName);
   string subject = configuration["Vapid:Subject"] ?? string.Empty;
   string publicKey = configuration["Vapid:PublicKey"] ?? string.Empty;
   string privateKey = configuration["Vapid:PrivateKey"] ?? string.Empty;
   var options = new Dictionary<string,object>();
   options["vapidDetails"] = new VapidDetails(subject, publicKey, privateKey);
   //options["gcmAPIKey"] = @"[your key here]";

   var webPushClient = new WebPushClient();

   var angularNotificationString = JsonSerializer.Serialize(new AngularNotification()
   {
      Notification = new() 
      {
         Body = message.Body,
         Title = message.Title,
         Icon = "assets/icons/icon-96x96.png"
      }
   }, new JsonSerializerOptions() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
   
   foreach (PushSubscription sub in subscriptions)
   {
      try
      {
         // fire and forget
         webPushClient.SendNotificationAsync(sub, angularNotificationString, options);
      }
      catch (WebPushException exception)
      {
         Console.WriteLine("Http STATUS code" + exception.StatusCode);
      }   
   }
});

app.Run();
