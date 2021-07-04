using System;

namespace TakeToTalk.Server.Model
{
    public class DtoMensagem : DtoPadrao
    {
        public string Mensagem { get; set; }
        public string Destinatario { get; set; }
        public string Sala { get; set; }
        public DateTime DataEnvio { get; set; }
        public DateTime DataRecebida { get; set; }
        public DateTime DataLeitura { get; set; }
    }
}
