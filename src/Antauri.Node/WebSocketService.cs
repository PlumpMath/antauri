using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class WebSocketService
{
    private static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
 
    private readonly RequestDelegate _next;
 
    public WebSocketService(RequestDelegate next)
    {
        _next = next;
    }
 
    public async Task Invoke(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(context);
            return;
        }
 
        CancellationToken ct = context.RequestAborted;
        WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
        var p2pService = (PeerToPeerService) context.RequestServices.GetService(typeof(PeerToPeerService));
        var socketId = Guid.NewGuid().ToString();
 
        _sockets.TryAdd(socketId, currentSocket);

        p2pService.AddPeer(currentSocket);
 
        while (currentSocket.State==WebSocketState.Open)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }
 
            var response = await currentSocket.ReceiveStringAsync(ct);
            if(string.IsNullOrEmpty(response))
            {
                if(currentSocket.State != WebSocketState.Open)
                {
                    break;
                }
 
                continue;
            }
            
            await p2pService.HandleMessage(currentSocket,response);
        }
 
        WebSocket dummy;
        _sockets.TryRemove(socketId, out dummy);
 
        await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
        currentSocket.Dispose();
    }
}