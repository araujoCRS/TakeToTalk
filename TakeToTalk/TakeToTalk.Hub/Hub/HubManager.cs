using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TakeToTalk.Hub.Protocolo;

namespace TakeToTalk.Hub.Hub
{
    public class HubManager
    {
        private HubService _hubService;
        public HubManager()
        {
            _hubService = new HubService();
        }

        public async Task Registre(string id, WebSocket webSocket, string group, Action<string, string> listenAction, Action<string> actionClose)
        {
            await _hubService.Add(id, webSocket);
            _hubService.GroupIn(group, id);
            await _hubService.Listen(id, webSocket, listenAction, actionClose);
        }

        public async Task UnRegistre(string id)
        {
            await _hubService.Remover(id);
        }

        public void Brodcast(Message message)
        {
            var ativos = _hubService.GetAll(message.Room);
            Parallel.ForEach(ativos, async socket =>
            {
                await _hubService.Send(socket, message.ToString());
            });
        }

        public async Task Send(Message message)
        {
            var socket = _hubService.Get(message.DestinyName, message.Room);
            await _hubService.Send(socket, message.ToString());
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
            return _hubService.GetRoomsConect(id);
        }
    }
}
