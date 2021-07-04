using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TakeToTalk.Server.Hub;

namespace TakeToTalk.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HubController : ControllerBase
    {
        private readonly ILogger<HubController> _logger;
        private HubService _hubService;
        public HubController(ILogger<HubController> logger, HubService hubService)
        {
            _logger = logger;
            _hubService = hubService;
        }

        [HttpGet]
        [Route("Send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Send()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _hubService.Add("teste", webSocket);
                await _hubService.SendTo(webSocket, "Conexão estabalecida. Pronto para receber");
                await _hubService.Add("teste", webSocket);
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket) //https://docs.microsoft.com/pt-br/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1#send-and-receive-messages
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
