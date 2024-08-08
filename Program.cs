using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

// Middleware to ensure only WebSocket requests are allowed
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
        await WebSocketHandler(websocket);
    }
    else {
        await next();
    }

});


async Task WebSocketHandler(WebSocket websocket)
{
    // Buffer of 4 KB
    var buffer = new byte[1024 * 4];

    // Data sent from client saved into buffer of 4KB
    WebSocketReceiveResult result = await websocket.ReceiveAsync(buffer, CancellationToken.None);

    // Creating the response message
    string MyResponse = "Hey!, I Got Your Message ✅";
    byte[] MyResponseBuffer = Encoding.UTF8.GetBytes(MyResponse);

    // Keep receiving data from client untill websocket is closed 
    while (!websocket.CloseStatus.HasValue)
    {
        // Send Acknowledgement
        await websocket.SendAsync(MyResponseBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        // Wait for next message
        result = await websocket.ReceiveAsync(buffer, CancellationToken.None);
    }

    // Close the websocket connection when while isnt running anymore
    await websocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

}

app.MapGet("/", () => "This is only for WebSocket Connections!!!");

app.Run();
