using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TakeToTalk.Server.Hub;
using TakeToTalk.Servicos.Negocio;

namespace TakeToTalk.Server.Chat
{
    public class Chat
    {
        private readonly HubManager _hub;
        private const string _salaGeral = "Geral";
        public Chat()
        {
            _hub = new HubManager();
            _hub.CreateGroup(_salaGeral);
        }
        public async Task Login(Usuario usuario, WebSocket webSocket)
        {
            _hub.Brodcast($"{usuario.Nickname} online",  _salaGeral);
            await _hub.Registre(usuario.Id, webSocket, MensageReceived);
        }

        private void MensageReceived(string message)
        {

        }
    }
}
