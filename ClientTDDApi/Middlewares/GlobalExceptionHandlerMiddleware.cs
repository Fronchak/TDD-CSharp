using ClientTDDApi.Exceptions;
using System.Net;
using System.Text.Json;

namespace ClientTDDApi.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(ApiException e)
            {
                context.Response.StatusCode = e.GetStatus();
                ExceptionResponse exceptionResponse = new ExceptionResponse()
                {
                    Error = e.GetError(),
                    Message = e.Message
                };
                string json = JsonSerializer.Serialize(exceptionResponse);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            }
            catch(Exception e)
            {
                context.Response.StatusCode = 500;
                ExceptionResponse exceptionResponse = new ExceptionResponse()
                {
                    Error = "Internal server error",
                    Message = "Something went wrong"
                };
                string json = JsonSerializer.Serialize(exceptionResponse);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            }
        }
    }
}
