using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TakeToTalk.Enumeradores.Hub;
using TakeToTalk.Hub.Protocolo;

namespace TakeToTalk.Hub.Hub
{
    /// <summary>
    /// Faz o controle de um cluster de Websockets independente. 
    /// </summary>
    public class HubManager
    {
        private readonly HubService _hubService;
        public HubManager()
        {
            _hubService = new HubService();
        }

        /// <summary>
        /// Vincula Websocket ao serviço de gerenciamento, classifica em um grupo e faz a associação das acões.
        /// </summary>
        /// <param name="id">Identifiador unico do Websocket</param>
        /// <param name="webSocket">Instancia Websocket com status Open</param>
        /// <param name="group">Grupo para primeira classificação do Websocket</param>
        /// <param name="listenAction">Ação que será executada quado uma mensagem for recebida pelo Websocket</param>
        /// <param name="actionClose">Ação que será executada quando a conexão com Websoket for fechada</param>
        /// <returns>Task de controle de execução</returns>
        public async Task Registre(string id, WebSocket webSocket, string group, Action<string, string> listenAction, Action<string> actionClose)
        {
            _hubService.Add(id, webSocket);
            _hubService.GroupIn(group, id);
            await _hubService.Listen(id, webSocket, listenAction, actionClose);
        }

        /// <summary>
        /// Remove vinculo do Websocket no serviço de gerenciamento.
        /// </summary>
        /// <param name="id">Identificador unico do Websocket</param>
        /// <returns>Tarefa de controle de execução</returns>
        public async Task UnRegistre(string id)
        {
            var removido = await _hubService.Remover(id);
            if (removido)
            {
                var message = new Message()
                {
                    Sender = id,
                    Action = EnumMessageActions.EXITCHAT,
                    Privacy = EnumMessagePrivacy.PUBLICA,
                    DestinyType = EnumMessageDestinyType.ROOM
                };

                Brodcast(message);
            }
        }

        /// <summary>
        /// Envia uma mensagem para todos que estejam vinculados a um grupo ou para todos os grupos se a flag 'allGroups' estiver marcada
        /// </summary>
        /// <param name="message">Mensagem com as informações de destino</param>
        /// <param name="allGroups">Controle de envio para grupo unico ou todos. False - Apenas grupo da mensagem e True - todos os grupos</param>
        public void Brodcast(Message message, bool allGroups = false)
        {
            var ativos = _hubService.GetAll(!allGroups? message.Room : null);
            Parallel.ForEach(ativos, async socket =>
            {
                await _hubService.Send(socket, message.ToString());
            });
        }

        public async Task Send(Message message)
        {
            var socket = _hubService.Get(message.User, message.Room);
            if(socket == null)
            {
                throw new Exception("Não existe um registro de saida vinculado aos dados informados");
            }
            await _hubService.Send(socket, message.ToString());
        }

        public bool IsRegistred(string id)
        {
            return _hubService.Get(id) != null;
        }

        public void CreateGroup(string group)
        {
            _hubService.GroupAdd(group);
        }

        public void ExitGroup(string group, string id)
        {
            _hubService.GroupOut(group, id);
        }

        public void JoinGroup(string group, string id)
        {
            _hubService.GroupIn(group, id);
        }

        public List<string> GetActive()
        {
            return _hubService.GetKeys();
        }
        public List<string> GetRooms()
        {
            return _hubService.GroupAll();
        }

        public List<string> GetRoomsConect(string id)
        {
            return string.IsNullOrEmpty(id) ? new List<string>() : _hubService.GroupAllocated(id);
        }
    }
}
