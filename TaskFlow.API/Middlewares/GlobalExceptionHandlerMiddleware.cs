using System.Net;
using System.Text.Json;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware(RequestDelegate requestDelegate,ILogger<GlobalExceptionHandlerMiddleware> logger) 
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await requestDelegate(context);
            }
            catch (Exception ex)
            { 
                logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,           // 404
                BusinessRuleException => (int)HttpStatusCode.BadRequest,     // 400
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                KeyNotFoundException => (int)HttpStatusCode.NotFound,        // 404
                ArgumentException => (int)HttpStatusCode.BadRequest,         // 400
                _ => (int)HttpStatusCode.InternalServerError                 // 500
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Detail = exception.StackTrace
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
