using System.Net;
using System.Text.Json;

namespace DigitalMarketplace.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the full exception on the server.
            // The client should not receive the stack trace.
            _logger.LogError(ex, "An unhandled exception occurred.");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException =>
                (int)HttpStatusCode.Unauthorized,

            InvalidOperationException =>
                (int)HttpStatusCode.BadRequest,

            KeyNotFoundException =>
                (int)HttpStatusCode.NotFound,

            _ =>
                (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            statusCode,
            message = exception switch
            {
                UnauthorizedAccessException =>
                    exception.Message,

                InvalidOperationException =>
                    exception.Message,

                KeyNotFoundException =>
                    exception.Message,

                _ =>
                    "An unexpected error occurred."
            }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}