using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TakeToTalk.Hub.Chat;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    public class HubController : ControllerPadrao
    {
        public HubController(Chat chat, ServicoUsuario servicoUsuario, ServicoSala servicoSala)
        : base(chat, servicoUsuario, servicoSala)
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

                await _chat.Login(new Usuario { Id = Usuario.Id, Nickname = Usuario.Nickname, Bio = Usuario.Bio }, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}
