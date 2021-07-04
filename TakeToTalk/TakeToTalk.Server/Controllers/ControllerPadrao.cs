using Microsoft.AspNetCore.Mvc;
using TakeToTalk.Server.Hub;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    public class ControllerPadrao : ControllerBase
    {
        protected HubService _hubService;
        protected ServicoUsuario _servicoUsuario;
        protected ServicoSala _servicoSala;
    }
}
