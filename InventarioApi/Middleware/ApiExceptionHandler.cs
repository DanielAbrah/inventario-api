using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InventarioApi.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ArgumentException)
            return false;

        logger.LogWarning(exception, "Solicitud rechazada por argumentos inválidos.");
        var status = StatusCodes.Status400BadRequest;
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = "Solicitud inválida",
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        }, cancellationToken);
        return true;
    }
}
