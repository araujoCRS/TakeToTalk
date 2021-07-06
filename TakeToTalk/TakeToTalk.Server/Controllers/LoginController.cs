using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using TakeToTalk.Hub.Chat;
using TakeToTalk.Server.Model;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    public class LoginController : ControllerPadrao
    {
        public LoginController(Chat chat, ServicoUsuario servicoUsuario, ServicoSala servicoSala)
        : base(chat, servicoUsuario, servicoSala)
        {
        }

        [HttpPost]
        [Route("Authenticate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DtoUsuario), StatusCodes.Status200OK)]
        public ActionResult<DtoUsuario> Authenticate([FromBody] DtoLogin credencial)
        {
            //AQUI SERIA GERADO UM TOKEN JWT
            //PARA SIMPLIFICAR ESTOU RETORNANDO O USUARIO
            var usuarios = _servicoUsuario.Consulte(x => x.Nickname == credencial.Username).ToList();
            if (!usuarios.Any())
            {
                return Unauthorized();
            }
            
            var json = JsonConvert.SerializeObject(usuarios.First());
            var fakeToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            return Ok(new { Usuario = usuarios.First(), Token = fakeToken });
        }
    }
}
