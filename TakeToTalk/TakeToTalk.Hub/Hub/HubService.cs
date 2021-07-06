using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TakeToTalk.Hub.Hub
{
    public class HubService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _socketsPool;
        private readonly ConcurrentDictionary<string, List<string>> _socketGroup;
        public HubService()
        {
            _socketsPool = new ConcurrentDictionary<string, WebSocket>();
            _socketGroup = new ConcurrentDictionary<string, List<string>>();
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
                _socketGroup.Values.ToList().ForEach(grupo => grupo.Remove(key));
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                return true;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Falha ao remover WebSocket", ex);
            }
        }

        public List<string> GetKeys()
        {
            WebSocket socket = null;
            var active = _socketsPool.Keys.Where(key => _socketsPool.TryGetValue(key, out socket) && socket.State == WebSocketState.Open);
            return active.ToList();
        }

        public bool GroupAdd(string group)
        {
            if(string.IsNullOrEmpty(group) || string.IsNullOrWhiteSpace(group) || _socketGroup.ContainsKey(group))
            {
                return false;
            }

            return _socketGroup.TryAdd(group, new List<string>());
        }
        public bool GroupIn(string group, string key)
        {
            try
            {
                WebSocket socket = null;
                List<string> grouped = null;

                if (!_socketsPool.TryGetValue(key, out socket))
                {
                    return false;
                }

                if (_socketGroup.ContainsKey(group))
                {
                    _socketGroup.TryGetValue(group, out grouped);
                    grouped.Add(key);
                    return true;
                }

                grouped = new List<string>() { key };
                _socketGroup.TryAdd(group, grouped);
                return true;
            }
            catch (OverflowException ex)
            {
                throw new Exception("Capacidade do pool de Socket atingido", ex);
            }
            catch(Exception ex)
            {
                throw new Exception("Ouve um erro inesperado", ex);
            }
        }

        public bool GroupOut(string group, string key)
        {
            try
            {
                if (_socketGroup.ContainsKey(group))
                {
                    List<string> grouped = null;
                    _socketGroup.TryGetValue(group, out grouped);
                    grouped.Remove(key);
                    return true;
                }

                return false;
            }
            catch (OverflowException ex)
            {
                throw new Exception("Capacidade do pool de Socket atingido", ex);
            }
        }

        public List<string> GroupAll()
        {
            return _socketGroup.Keys.ToList();
        }

        public List<string> GetRoomsConect(string key)
        {
            List<string> grouped = null;
            var groups = _socketGroup.Keys.ToList();
            var groupsKey = groups.Where(x => _socketGroup.TryGetValue(x, out grouped) && grouped.Contains(key));
            return groupsKey.ToList();
        }

        public WebSocket Get(string key, string group = null)
        {
            WebSocket socket = null;
            List<string> grouped = null;

            _socketsPool.TryGetValue(key, out socket);
            if (string.IsNullOrEmpty(group))
            {
                return socket;
            }

            _socketGroup.TryGetValue(group, out grouped);
            return grouped.Contains(key) ? socket : null;
        }

        public ConcurrentBag<WebSocket> GetAll(string group = null)
        {
            var bag = new ConcurrentBag<WebSocket>();
            List<string> grouped = null;
            WebSocket socket = null;

            if (group != null && _socketGroup.TryGetValue(group, out grouped))
            {
                grouped.ForEach(key =>
                {
                    _socketsPool.TryGetValue(key, out socket);
                    bag.Add(socket);
                });

                return bag;
            }

            _socketsPool.Values.ToList().ForEach(socket => bag.Add(socket));
            return bag;
        }

        public async Task Send(WebSocket webSocket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket?.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task Listen(string id, WebSocket webSocket, Action<string, string> actionReceived, Action<string> actionClose)
        {
            var buffer = new byte[1024 * 4];

            while (true)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    actionReceived(Encoding.UTF8.GetString(buffer, 0, result.Count), id);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    actionClose(id);
                    break;
                }
            }
        }
    }
}
