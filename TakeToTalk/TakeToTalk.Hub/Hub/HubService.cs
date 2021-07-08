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
    /// <summary>
    /// Cria um pool de websockets e controla seu ciclo de vida.
    /// Faz o agrupamentos de websokets por nomes exlusivos
    /// Controla a saida e o recebimento de mensagens
    /// </summary>
    public class HubService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _socketsPool;
        private readonly ConcurrentDictionary<string, List<string>> _socketGroup;
        public HubService()
        {
            _socketsPool = new ConcurrentDictionary<string, WebSocket>();
            _socketGroup = new ConcurrentDictionary<string, List<string>>();
        }

        /// <summary>
        /// Adiciona Websocket ao Pool para orquestração e associa seu identificador unico.
        /// </summary>
        /// <param name="key">Identificador unico</param>
        /// <param name="socket">Instancia de Websocket com status de conexão 'Open'</param>
        /// <returns></returns>
        public bool Add(string key, WebSocket socket)
        {
            try
            {
                if(socket.State != WebSocketState.Open)
                {
                    throw new Exception("Status da conexão deve ser 'Open'");
                }

                if (_socketsPool.ContainsKey(key))
                {
                    throw new Exception("Ja existe um registro para o identificador informado");
                }

                return _socketsPool.TryAdd(key, socket);
            }
            catch (OverflowException ex)
            {
                throw new Exception("Capacidade do pool de Socket atingido", ex);
            }
        }

        /// <summary>
        /// Remover Websocket e finalizar comunicação
        /// </summary>
        /// <param name="key">Identificador unico do Websocket</param>
        /// <returns>True se foi removido e encerrado. False caso contrario</returns>
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
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }

                return true;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Falha ao remover WebSocket", ex);
            }
        }

        /// <summary>
        /// Obtem todos os identificadores de Websockets
        /// </summary>
        /// <returns>Lista de identificadores</returns>
        public List<string> GetKeys()
        {
            WebSocket socket = null;
            var active = _socketsPool.Keys.Where(key => _socketsPool.TryGetValue(key, out socket) && socket.State == WebSocketState.Open);
            return active.ToList();
        }

        /// <summary>
        /// Crar novo grupo
        /// </summary>
        /// <param name="group">Identificador unico do grupo</param>
        /// <returns>True se o grupo foi criado. False caso contrario.</returns>
        public bool GroupAdd(string group)
        {
            if (string.IsNullOrEmpty(group) || string.IsNullOrWhiteSpace(group) || _socketGroup.ContainsKey(group))
            {
                return false;
            }

            return _socketGroup.TryAdd(group, new List<string>());
        }

        /// <summary>
        /// Adiciona o vinculo de um Websocket com um grupo
        /// </summary>
        /// <param name="group">Identificador fo grupo</param>
        /// <param name="key">Identificador unico do Websocket</param>
        /// <returns>True caso tenha sido adicionado. False caso contrario</returns>
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
            catch (Exception ex)
            {
                throw new Exception("Ouve um erro inesperado", ex);
            }
        }

        /// <summary>
        /// Remove o vinculo de um Websocket com um grupo
        /// </summary>
        /// <param name="group">Identificador fo grupo</param>
        /// <param name="key">Identificador unico do Websocket</param>
        /// <returns>True caso tenha sido removido. False caso contrario</returns>
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

        /// <summary>
        /// Consulta todos os grupos existentes
        /// </summary>
        /// <returns>Lista de grupo</returns>
        public List<string> GroupAll()
        {
            return _socketGroup.Keys.ToList();
        }

        /// <summary>
        /// Obter lista de grupos que o Websocket esta alocado
        /// </summary>
        /// <param name="key">Identificador unico do Websocket</param>
        /// <returns>Lista de grupos</returns>
        public List<string> GroupAllocated(string key)
        {
            List<string> grouped = null;
            var groups = _socketGroup.Keys.ToList();
            var groupsKey = groups.Where(x => _socketGroup.TryGetValue(x, out grouped) && grouped.Contains(key));
            return groupsKey.ToList();
        }

        /// <summary>
        /// Busca Websocket pelo identifiador. Caso informe o grupo será aplicado como filtro.
        /// </summary>
        /// <param name="key">Idenficador unico do Websocket</param>
        /// <param name="group">Grupo para filtragem</param>
        /// <returns>Websocket correspondente ao idenficador e ao grupo se foi informado</returns>
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

        /// <summary>
        /// Obetenha todos os Websockets do Pool ou de um grupo especifico
        /// </summary>
        /// <param name="group">Grupo para filtagem</param>
        /// <returns>Lista de Websockets disponiveis</returns>
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

        /// <summary>
        /// Envia uma mensagem para cliete do Websocket
        /// </summary>
        /// <param name="webSocket">Websoket com a conexão em status Open</param>
        /// <param name="message">Mensagem que será envida</param>
        /// <returns>Task de controle de execução</returns>
        public async Task Send(WebSocket webSocket, string message)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                throw new Exception("Websocket não esta com a conexão aberta.");
            }
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket?.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Mantem uma escuta continua no Websocket e dispara as Actions.
        /// </summary>
        /// <param name="id">Identifiador unico do Websocket</param>
        /// <param name="webSocket">Instancia Websocket com status Open</param>
        /// <param name="actionReceived">Ação que será executada quado uma mensagem for recebida pelo Websocket</param>
        /// <param name="actionClose">Ação que será executada quando a conexão com Websoket for fechada</param>
        /// <returns>Task de controle de execução</returns>
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
