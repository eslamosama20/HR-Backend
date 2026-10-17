using SmartAndPro.ERP.Infrastructure.WebSockets;

namespace SmartAndPro.ERP.WebAPI.Middlewares
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketConnectionManager _manager;

        public WebSocketMiddleware(RequestDelegate next, WebSocketConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var userId = context.Request.Query["userId"].ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        _manager.AddSocket(userId, socket);
                    }

                    await _manager.ListenAsync(userId, socket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}

