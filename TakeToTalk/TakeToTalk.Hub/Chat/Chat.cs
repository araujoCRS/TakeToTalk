using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TakeToTalk.Enumeradores.Hub;
using TakeToTalk.Hub.Hub;
using TakeToTalk.Hub.Protocolo;
using TakeToTalk.Servicos.Negocio;

namespace TakeToTalk.Hub.Chat
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
            _hub.Brodcast($"{usuario.Nickname} esta online",  _salaGeral);
            await _hub.Registre(usuario.Nickname, webSocket, MensageReceived, _salaGeral);
        }

        private void MensageReceived(string data, string usuario)
        {
            var message = JsonConvert.DeserializeObject<Message>(data);
            switch (message.Action)
            {
                case EnumMessageActions.NONE:
                    SendMessage(message);
                    return;
                case EnumMessageActions.NEWCROOM:
                    CreateRoom(message.Value);
                    return;
                case EnumMessageActions.JOINROOM:
                    JoinRoom(usuario, message.Value);
                    return;
                case EnumMessageActions.EXITROOM:
                    ExitRoom(usuario, message.Value);
                    return;
                case EnumMessageActions.EXITCHAT:
                    ExitChat(usuario);
                    return;
            }
        }

        private void SendMessage(Message message)
        {
            if(message.Destiny == EnumMessageDestiny.ROOM && message.Privacy == EnumMessagePrivacy.PUBLICA)
            {
                _hub.Brodcast(message.Value, message.Room);
                return;
            }
            if (message.Destiny == EnumMessageDestiny.USER)
            {
                Task.WaitAll(_hub.Send(message.Value, message.DestinyName, message.Room));
                return;
            }
        }

        private void ExitChat(string usuario)
        {
            Task.WaitAll(_hub.UnRegistre(usuario));
            _hub.Brodcast($"{usuario} ficou offline", _salaGeral);
        }

        private void ExitRoom(string usuario, string value)
        {
            _hub.ExitGroup(value, usuario);
        }

        private void JoinRoom(string usuario, string value)
        {
            _hub.JoinGroup(usuario, value);
        }

        private void CreateRoom(string value)
        {
            _hub.CreateGroup(value);
        }
    }
}