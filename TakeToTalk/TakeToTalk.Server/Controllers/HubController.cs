using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TakeToTalk.Server.Hub;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HubController : ControllerPadrao
    {
        public HubController(HubService hubService, ServicoUsuario servicoUsuario, ServicoSala servicoSala)
        {
            _hubService = hubService;
            _servicoUsuario = servicoUsuario;
            _servicoSala = servicoSala;
        }

        public ActionResult Cadastrar(DtoUsuario usuario)
        {
            try
            {
                if(usuario == null)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Dados invalidos"
                    });
                }

                var matchs = _servicoUsuario.Consulte(x => x.Nicknome == usuario.Nickname);
                if (matchs.Any())
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Nome escolhido esta em uso."
                    });
                }

                //Implementar um conversor no serviço se sobrar tempo
                var negocio = new Usuario()
                {
                    Nicknome = usuario.Nickname,
                    Bio = usuario.Bio
                };
                _servicoUsuario.Salve(negocio);

                return Ok(negocio);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro no cadastro de usuario.", ex);
            }
        }

        [HttpGet]
        [Route("Send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Send()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _hubService.Add(Guid.NewGuid().ToString(), webSocket);
                await _hubService.SendTo(webSocket, "Conexão estabalecida. Pronto para receber");
                await _hubService.Listen(webSocket, RouterMessage);
                //await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async void RouterMessage(string messsage)
        {
            var webSocket = _hubService.Get("teste");
            await _hubService.SendTo(webSocket, $"Sua mensagem: '{messsage}'");
        }
    }
}
