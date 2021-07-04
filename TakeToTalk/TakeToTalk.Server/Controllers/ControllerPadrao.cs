using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakeToTalk.Server.Hub;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ControllerPadrao : ControllerBase
    {
        protected HubService _hubService;
        protected ServicoUsuario _servicoUsuario;
        protected ServicoSala _servicoSala;
        protected DtoUsuario Usuario;

        public ControllerPadrao(HubService hubService, ServicoUsuario servicoUsuario, ServicoSala servicoSala)
        {
            _hubService = hubService;
            _servicoUsuario = servicoUsuario;
            _servicoSala = servicoSala;
        }

        protected bool Autenticar(string token)
        {
            //EM UM FLUXO COMPLETO EU OPTARIA POR USAR UM TOKEN JWT.
            //ASSIM ANTES SE CONECTAR AO SOCKET OBRIGATORIAMENTE UM LOGIN TERIA QUE SER FEITO API
            //COMO ESTE É UM MOCK IREI APENAS DESSERIALIZAR UM JSON

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            Usuario = JsonConvert.DeserializeObject<DtoUsuario>(json);
            return true;
        }
    }
}
