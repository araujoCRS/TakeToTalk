using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TakeToTalk.Server.Hub
{
    public class HubService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _socketsPool;
        private readonly ConcurrentDictionary<string, List<WebSocket>> _socketGroup;
        public HubService()
        {
            _socketsPool = new ConcurrentDictionary<string, WebSocket>();
            _socketGroup = new ConcurrentDictionary<string, List<WebSocket>>();
        }
        public async Task<bool> Add(string key, WebSocket socket)
        {
            try
            {
                if (_socketsPool.ContainsKey(key))
                {
                    await Remover(key);
                }
                return _socketsPool.TryAdd(key, socket);
            }
            catch (OverflowException ex)
            {
                throw new Exception("Capacidade do pool de Socket atingido", ex);
            }
        }
        public async Task<bool> Remover(string key)
        {
            try
            {
                WebSocket socket = null;
                _socketsPool.TryRemove(key, out socket);
                if (socket == null)
                {
                    return false;
                }
                _socketGroup.Values.ToList().ForEach(grupo => grupo.Remove(socket));
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexao interrompida", CancellationToken.None);
                return true;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Falha ao remover WebSocket", ex);
            }
        }
        public bool GroupTo(string group, string key)
        {
            try
            {
                WebSocket socket = null;
                List<WebSocket> groupSocket = null;

                if (!_socketsPool.TryGetValue(key, out socket))
                {
                    return false;
                }

                if (_socketGroup.ContainsKey(group))
                {
                    _socketGroup.TryGetValue(group, out groupSocket);
                    groupSocket.Add(socket);
                    return true;
                }

                groupSocket = new List<WebSocket>() { socket };
                _socketGroup.TryAdd(group, groupSocket);
                return true;
            }
            catch (OverflowException ex)
            {
                throw new Exception("Capacidade do pool de Socket atingido", ex);
            }
        }

        public WebSocket Get(string key)
        {
            WebSocket socket = null;
            _socketsPool.TryGetValue(key, out socket);
            return socket;
        }

        public ConcurrentBag<WebSocket> GetAll()
        {
            var bag = new ConcurrentBag<WebSocket>();
            _socketsPool.Values.ToList().ForEach(socket => bag.Add(socket));
            return bag;
        }

        public ConcurrentBag<WebSocket> GetAll(string group)
        {
            var bag = new ConcurrentBag<WebSocket>();
            List<WebSocket> all = null;

            if (_socketGroup.TryGetValue(group, out all))
            {
                all.ForEach(socket => bag.Add(socket));
            }

            return bag;
        }

        public async Task SendTo(WebSocket webSocket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task Listen(WebSocket webSocket, Action<string> action)
        {
            var buffer = new byte[1024 * 4];

            while (true)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    action(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }
            }
        }
    }
}
