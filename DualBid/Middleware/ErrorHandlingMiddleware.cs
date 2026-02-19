using DualBid.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace DualBid.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 1) Información de contexto 
                var routeWhereExceptionOccured = context.Request.Path.Value ?? "N/A";
                var eventId = $"{Guid.NewGuid():N}-{DateTime.Now: yyMMddHHmmss}";

                // 2) Construir respuesta para UI 
                var result = new ErrorMiddlewareViewModel
                {
                    Path = routeWhereExceptionOccured,
                    IdEvent = eventId,
                    ListMessages = GetMessages(ex)
                };

                // 3) Log detallado (no se envía al usuario) 
                var sb = new StringBuilder();
                sb.AppendLine($"EventId    : {eventId} ");
                sb.AppendLine($"Path       : {routeWhereExceptionOccured} ");
                sb.AppendLine($"ErrorList  : {string.Join(" | ", result.ListMessages)} ");
                sb.AppendLine($"StackTrace : {ex} ");

                _logger.LogError(sb.ToString());

                // 4) Serializar 
                var messagesJson = JsonSerializer.Serialize(result);

                // 5) Encode para URL (evita romper la URL y reduce riesgo) 
                var redirectUrl = QueryHelpers.AddQueryString("/Home/ErrorHandler","messagesJson", messagesJson);
                context.Response.Redirect(redirectUrl);
            }
        }

        private static List<string> GetMessages(Exception ex)
        {
            if (ex is AggregateException ae)
                return ae.InnerExceptions.Select(e => e.Message).ToList();

            return new List<string> { ex.Message };
        }
    }

}
