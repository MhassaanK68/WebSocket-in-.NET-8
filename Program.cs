using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serve static files from wwwroot
app.UseStaticFiles();
app.UseDefaultFiles();

// Enable WebSocket support
app.UseWebSockets();

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

// Middleware to handle WebSocket requests
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await WebSocketHandler(webSocket);
    }
    else
    {
        await next();
    }
});

async Task WebSocketHandler(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];

    string responseMessage = "Hey!, I Got Your Message ✅";
    byte[] responseBuffer = Encoding.UTF8.GetBytes(responseMessage);

    WebSocketReceiveResult result;
    do
    {
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Text)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    } while (!result.CloseStatus.HasValue);

    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

app.Run();
