using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Infrastructure.WebSockets
{
    public class WebSocketConnectionManager
    {
        private readonly Dictionary<string, WebSocket> _sockets = new();

        public void AddSocket(string userId, WebSocket socket)
        {
            _sockets[userId] = socket;
        }

        public async Task ListenAsync(string userId, WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _sockets.Remove(userId);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancellationToken.None);
                }
            }
        }

        public async Task SendAsync(string userId, string message)
        {
            if (_sockets.ContainsKey(userId))
            {
                var socket = _sockets[userId];
                if (socket.State == WebSocketState.Open)
                {
                    var bytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task BroadcastAsync(string message)
        {
            foreach (var socket in _sockets.Values)
            {
                if (socket.State == WebSocketState.Open)
                {
                    var bytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
