using System.Text.Json;
using WebPush;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
const string fileName = "subscriptions.json";
app.MapGet("/subscriptions", async () => await PushSubscriptions(fileName));

app.MapPost("/subscriptions", async (PushSubscription subscription) =>
{
   var subscriptions = await PushSubscriptions(fileName);
   subscriptions.Add(subscription);
   
   File.WriteAllText(fileName,JsonSerializer.Serialize(subscriptions));
});

app.Run();

async Task<List<PushSubscription>> PushSubscriptions(string filename)
{
   if (!File.Exists(fileName))
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
