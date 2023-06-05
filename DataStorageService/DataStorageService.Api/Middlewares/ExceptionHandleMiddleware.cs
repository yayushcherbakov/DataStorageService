using System.Net;
using DataStorageService.Api.Constants;

namespace DataStorageService.Api.Middlewares;

/// <summary>
/// Level for exception handling.
/// </summary>
public class ExceptionHandleMiddleware
{
    /// <summary>
    /// Passes control to the next intermediary.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The ExceptionHandleMiddleware constructor is used to create an instance of the mediator
    /// and to set a value for the _next field that will be used to call the next mediator in the request chain.
    /// </summary>
    public ExceptionHandleMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Passes control to the next intermediary in the request processing chain.
    /// </summary>
    /// <param name="context">The current HTTP context of the request and response.</param>
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
    
    /// <summary>
    /// Handles the default exception and returns the corresponding HTTP response.
    /// </summary>
    /// <param name="context">The current HTTP context of the request and response.</param>
    /// <param name="httpStatusCode">HTTP status code as per RFC 2616.</param>
    /// <param name="message">Message about the result of the request.</param>
    private static async Task HandleDefaultException(HttpContext context, HttpStatusCode httpStatusCode,
        string message)
    {
        context.Response.Clear();
        context.Response.StatusCode = (int)httpStatusCode;
        context.Response.ContentType = @"application/json";
        await context.Response.WriteAsync(message);
    }
}
