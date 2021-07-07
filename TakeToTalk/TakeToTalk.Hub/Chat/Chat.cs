using Newtonsoft.Json;
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
        private const string _defaultRoom = "Geral";
        public Chat()
        {
            _hub = new HubManager();
            _hub.CreateGroup(_defaultRoom);
        }
        public async Task Login(Usuario usuario, WebSocket webSocket)
        {
            var message = new Message()
            {
                Sender = usuario.Nickname,
                Action = EnumMessageActions.JOINCHAT,
                Privacy = EnumMessagePrivacy.PUBLICA,
                DestinyType = EnumMessageDestinyType.ROOM
            };

            await _hub.UnRegistre(usuario.Nickname);

            var rooms = _hub.GetRooms();
            rooms.ForEach(room =>
            {
                message.Room = room;
                _hub.Brodcast(message);
            });

            message.Action = EnumMessageActions.JOINROOM;
            message.Room = _defaultRoom;
            _hub.Brodcast(message);

            await _hub.Registre(usuario.Nickname, webSocket, _defaultRoom, MensageReceived, WebsocketCloseForced);
        }

        private void MensageReceived(string data, string usuario)
        {
            var message = JsonConvert.DeserializeObject<Message>(data);
            switch (message.Action)
            {
                case EnumMessageActions.TALK:
                    SendMessage(message);
                    return;
                case EnumMessageActions.NEWCROOM:
                    CreateRoom(usuario, (string)message.Data);
                    return;
                case EnumMessageActions.JOINROOM:
                    JoinRoom(usuario, (string)message.Data);
                    return;
                case EnumMessageActions.EXITROOM:
                    ExitRoom(usuario, (string)message.Data);
                    return;
                case EnumMessageActions.EXITCHAT:
                    ExitChat(usuario);
                    return;
                case EnumMessageActions.STATUS:
                    GetStatus(message);
                    return;
            }
        }

        private void WebsocketCloseForced(string usuario)
        {
            ExitChat(usuario);
        }

        private void GetStatus(Message message)
        {
            var usuarios = _hub.GetActive();
            var rooms = _hub.GetRooms();
            var roomsConect = _hub.GetRoomsConect(message.Sender);

            message.DestinyType = EnumMessageDestinyType.USER;
            message.Action = EnumMessageActions.STATUS;
            message.Data = new { usuarios, rooms, roomsConect };
            message.User = message.Sender;
            message.Privacy = EnumMessagePrivacy.PRIVADA;

            SendMessage(message);
        }

        private void SendMessage(Message message)
        {
            if (message.DestinyType == EnumMessageDestinyType.USER)
            {
                Task.WaitAll(_hub.Send(message));
                return;
            }

            if (message.Privacy == EnumMessagePrivacy.PUBLICA && string.IsNullOrEmpty(message.User))
            {
                _hub.Brodcast(message);
                return;
            }

            if (!string.IsNullOrEmpty(message.User))
            {
                if (message.DestinyType == EnumMessageDestinyType.ROOM)
                {
                    var rooms = _hub.GetRoomsConect(message.User);
                    if (!rooms.Contains(message.Room)) //SE USUARIO NÃO ESTA NA SALA
                    {
                        message.Action = EnumMessageActions.ERROR;
                        message.User = message.Sender;
                        Task.WaitAll(_hub.Send(message));
                        return;
                    }
                    if(message.Privacy == EnumMessagePrivacy.PRIVADA)
                    {
                        Task.WaitAll(_hub.Send(message));
                        message.Data = null;
                    }

                    _hub.Brodcast(message);
                    return;
                }
            }
        }

        private void ExitChat(string usuario)
        {
            var rooms = _hub.GetRoomsConect(usuario);
            Task.WaitAll(_hub.UnRegistre(usuario));

            var message = new Message()
            {
                Sender = usuario,
                Action = EnumMessageActions.EXITCHAT,
                Privacy = EnumMessagePrivacy.PUBLICA,
                DestinyType = EnumMessageDestinyType.ROOM
            };
            
            rooms.ForEach(room =>
            {
                message.Room = room;
                _hub.Brodcast(message);
            });
        }

        private bool ExitRoom(string usuario, string room)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(room))
            {
                return false;
            }

            var message = new Message()
            {
                Sender = usuario,
                Action = EnumMessageActions.EXITROOM,
                DestinyType = EnumMessageDestinyType.ROOM,
                Privacy = EnumMessagePrivacy.PUBLICA,
                Room = room
            };
            _hub.ExitGroup(room, usuario);
            SendMessage(message);
            return true;
        }

        private void JoinRoom(string usuario, string room)
        {
            var message = new Message()
            {
                Sender = usuario,
                Action = EnumMessageActions.JOINROOM,
                DestinyType = EnumMessageDestinyType.ROOM,
                Privacy = EnumMessagePrivacy.PUBLICA,
                Room = room
            };
            _hub.JoinGroup(room, usuario);
            SendMessage(message);
        }

        private void CreateRoom(string usuario, string room)
        {
            var rooms = _hub.GetRooms();
            _hub.CreateGroup(room);

            var message = new Message()
            {
                Sender = usuario,
                Action = EnumMessageActions.NEWCROOM,
                DestinyType = EnumMessageDestinyType.ROOM,
                Privacy = EnumMessagePrivacy.PUBLICA,
                Data = room
            };
            rooms.ForEach(room =>
            {
                message.Room = room;
                _hub.Brodcast(message);
            });
        }
    }
}