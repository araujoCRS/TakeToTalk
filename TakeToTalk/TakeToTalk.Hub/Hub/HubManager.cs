using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace TakeToTalk.Hub.Hub
{
    public class HubManager
    {
        private HubService _hubService;
        public HubManager()
        {
            _hubService = new HubService();
        }

        public async Task Registre(string id, WebSocket webSocket, Action<string, string> listenAction, string group)
        {
            await _hubService.Add(id, webSocket);
            _hubService.GroupIn(group, id);
            await _hubService.Listen(id, webSocket, listenAction);
        }

        public async Task UnRegistre(string id)
        {
            await _hubService.Remover(id);
        }

        public void Brodcast(string message, string group = null)
        {
            var ativos = _hubService.GetAll(group);
            Parallel.ForEach(ativos, async socket =>
            {
                await _hubService.SendTo(socket, message);
            });
        }

        public async Task Send(string message, string id, string group = null)
        {
            var socket = _hubService.Get(id, group);
            await _hubService.SendTo(socket, message);
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
    }
}
