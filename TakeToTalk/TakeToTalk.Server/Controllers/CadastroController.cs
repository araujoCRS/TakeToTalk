using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TakeToTalk.Server.Hub;
using TakeToTalk.Server.Model;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Servico;

namespace TakeToTalk.Server.Controllers
{
    public class CadastroController : ControllerPadrao
    {
        public CadastroController(HubService hubService, ServicoUsuario servicoUsuario, ServicoSala servicoSala)
        : base(hubService, servicoUsuario, servicoSala)
        {
        }

        [HttpPost]
        [Route("ManterUsuario")]
        [ProducesResponseType(typeof(DtoUsuario), StatusCodes.Status200OK)]
        public ActionResult ManterUsuario(DtoUsuario usuario)
        {
            try
            {
                if (usuario == null)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Dados invalidos"
                    });
                }

                var matchs = _servicoUsuario.Consulte(x => x.Nickname == usuario.Nickname);
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
                    Id = usuario.Id,
                    Nickname = usuario.Nickname,
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

        [HttpPost]
        [Route("ManterSala")]
        [ProducesResponseType(typeof(DtoSala), StatusCodes.Status200OK)]
        public ActionResult ManterSala(DtoSala sala)
        {
            try
            {
                if (sala == null)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Dados invalidos"
                    });
                }

                var matchs = _servicoSala.Consulte(x => x.Nome == sala.Nome);
                if (matchs.Any())
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Nome escolhido esta em uso."
                    });
                }

                //Implementar um conversor no serviço se sobrar tempo
                var negocio = new Sala()
                {
                    Id = sala.Id,
                    Nome = sala.Nome
                };
                _servicoSala.Salve(negocio);

                return Ok(negocio);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro no cadastro de usuario.", ex);
            }
        }
    }
}
