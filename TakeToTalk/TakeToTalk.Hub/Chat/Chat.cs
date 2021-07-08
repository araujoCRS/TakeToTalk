using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TakeToTalk.Enumeradores.Hub;
using TakeToTalk.Hub.Hub;
using TakeToTalk.Hub.Protocolo;
using TakeToTalk.Servicos.Negocio;

namespace TakeToTalk.Hub.Chat
{
    /// <summary>
    /// Controla a troca de mensagens e informações das instancias conectadas.
    /// Abstrai os conceitos de uma de um bate-popo virtual e implementa as regras de troca de mensagens.
    /// </summary>
    public class Chat
    {
        private readonly HubManager _hub;
        private const string _defaultRoom = "Geral";
        public Chat()
        {
            _hub = new HubManager();
            _hub.CreateGroup(_defaultRoom);
        }

        /// <summary>
        /// Regista nova conexão de usuario e informa todos do chat sobre novo membro online
        /// </summary>
        /// <param name="usuario">Dados do usuario</param>
        /// <param name="webSocket">Websoket de comunicação com 'client' do usuario</param>
        /// <returns>Tarefa de controle de execução</returns>
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
            _hub.Brodcast(message, true);

            message.Action = EnumMessageActions.JOINROOM;
            message.Room = _defaultRoom;
            _hub.Brodcast(message);
            
            if (_hub.IsRegistred(usuario.Nickname))
            {
                LeaveChat(usuario.Nickname);
            }

            await _hub.Registre(usuario.Nickname, webSocket, _defaultRoom, MensageReceived, WebsocketCloseForced);
        }

        /// <summary>
        /// Acão usada no vinculo do Websocket atribuida ao evento de nova mensagem.
        /// Recebe os dados das mensagens e traduz para ações de controle
        /// </summary>
        /// <param name="data">Texto com a mensagem recebida via Websocket</param>
        /// <param name="usuario">Identificador do remetente da mensagem</param>
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
                    LeaveRoom(usuario, (string)message.Data);
                    return;
                case EnumMessageActions.EXITCHAT:
                    LeaveChat(usuario);
                    return;
                case EnumMessageActions.STATUS:
                    GetStatus(message);
                    return;
            }
        }

        private void WebsocketCloseForced(string usuario)
        {
            LeaveChat(usuario);
        }

        /// <summary>
        /// Encapsula todos os dados de usuarios conectados e salas disponiveis e devolve ao solicitante
        /// </summary>
        /// <param name="message">Mensagem do solicitante contendo os dados do remetente</param>
        private void GetStatus(Message message) //EM CASOS MAIS COMPLEXOS, ESSA FUNÇÃO PODE SER DECOMPOSTA EX.: status da sala, do usuario x, etc
        {
            var usuarios = _hub.GetActive();
            var rooms = _hub.GetRooms();
            var connected = _hub.GetRoomsConect(message.Sender);

            message.DestinyType = EnumMessageDestinyType.USER;
            message.Action = EnumMessageActions.STATUS;
            message.Data = new { usuarios, rooms, connected };
            message.User = message.Sender;
            message.Privacy = EnumMessagePrivacy.PRIVADA;

            SendMessage(message);
        }

        /// <summary>
        /// Trata uma solicitação de envio de mensagem. Redireciona para os devidos destintarios de acordo com os parâmetros da solicitação.
        /// </summary>
        /// <param name="message">Mensagem do solicitante contendo os parâmetros de envio</param>
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

        /// <summary>
        /// Encerra conexão do usuario e avisa todos os interessados do chat
        /// </summary>
        /// <param name="usuario">Ientificador unico do usuario</param>
        private void LeaveChat(string usuario)
        {
            var message = new Message()
            {
                Sender = usuario,
                Action = EnumMessageActions.EXITCHAT,
                Privacy = EnumMessagePrivacy.PUBLICA,
                DestinyType = EnumMessageDestinyType.ROOM
            };

            Task.WaitAll(_hub.UnRegistre(usuario));
            _hub.Brodcast(message, true);
        }

        /// <summary>
        /// Remove usuario da sala e avisa todos da mesma
        /// </summary>
        /// <param name="usuario">Identificador unico do usuario</param>
        /// <param name="room">Sala que o usuario esta vinculado</param>
        /// <returns>True se for removido. False caso contrario</returns>
        private bool LeaveRoom(string usuario, string room)
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