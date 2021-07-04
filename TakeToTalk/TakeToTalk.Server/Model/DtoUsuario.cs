using TakeToTalk.Server.Model;

namespace TakeToTalk.Servicos.Negocio
{
    public class DtoUsuario : DtoPadrao
    {
        public string Nickname { get; set; }
        public string Bio { get; set; }
    }
}
