using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TakeToTalk.Server.Hub;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    public class HubController : ControllerPadrao
    {
        public HubController(HubService hubService, ServicoUsuario servicoUsuario, ServicoSala servicoSala)
        : base(hubService, servicoUsuario, servicoSala)
        {
        }

        [HttpGet]
        [Route("Send/{token}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Send(string token)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (!Autenticar(token))
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Autenticação invalida", CancellationToken.None);
                    return;
                }

                await _hubService.Add(Usuario.Id, webSocket);
                await _hubService.Listen(webSocket, RouterMessage);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async void RouterMessage(string messsage)
        {
            var webSocket = _hubService.Get(Usuario.Id);
            await _hubService.SendTo(webSocket, $"Sua mensagem: '{messsage}'");
        }
    }
}
