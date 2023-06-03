using System.Net;
using DataStorageService.Api.Constants;

namespace DataStorageService.Api.Middlewares;

public class ExceptionHandleMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandleMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApplicationException exception)
        {
            await HandleDefaultException(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await HandleDefaultException(context, HttpStatusCode.Unauthorized, exception.Message);
        }
        catch (Exception exception)
        {
            await HandleDefaultException(context, HttpStatusCode.InternalServerError,
                GeneralErrorMessages.ServerIsNotResponding);
        }
    }

    private static async Task HandleDefaultException(HttpContext context, HttpStatusCode httpStatusCode,
        string message)
    {
        context.Response.Clear();
        context.Response.StatusCode = (int)httpStatusCode;
        context.Response.ContentType = @"application/json";
        await context.Response.WriteAsync(message);
    }
}
