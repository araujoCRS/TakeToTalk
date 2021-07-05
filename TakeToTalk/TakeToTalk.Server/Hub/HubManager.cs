using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TakeToTalk.Server.Hub
{
    public class HubManager
    {
        private HubService _hubService;
        public HubManager()
        {
            _hubService = new HubService();
        }

        public async Task Registre(string id, WebSocket webSocket, Action<string> listenAction)
        {
            await _hubService.Add(id, webSocket);
            await _hubService.Listen(webSocket, listenAction);
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
    }
}
