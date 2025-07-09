using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ProjectTemplate.Application.Common.Constants;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Shared.Constants;
using Serilog;

namespace ProjectTemplate.Application.Common.Exceptions;
public sealed class GlobalExceptionHandler(IHostEnvironment hostEnvironment) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(
       HttpContext httpContext,
       Exception exception,
       CancellationToken cancellationToken)
    {
        var responseMessage = $"Title:{GetTitle(exception)}: Message:{exception.Message}";
        var (result, writeLog) = exception switch
        {

            ArgumentNullException or ModelIsNullException => (Results.Conflict(responseMessage), false),
            NotFoundException or FileNotFoundException => (Results.NotFound(responseMessage), false),
            ValidationException => (Results.UnprocessableEntity(responseMessage), true),
            AlreadyExistException or ConflictException or JsonSerializationException => (Results.Conflict(responseMessage), true),
            AccessDeniedException => (Results.Forbid(), true),
            ErrorFromClientException => (Results.BadRequest(responseMessage), true),
            RefreshTokenExpiredException => (Results.Unauthorized(), false),
            _ => (Results.InternalServerError(responseMessage), true)
        };
        if (writeLog)
        {

            var userId = httpContext.User.Claims.FirstOrDefault(x => x.Type == StaticClaims.UserId)?.Value ?? "Unauthorized user";

            var errorMessageForTelegram = $"""

                              🚨Error Log🚨

                              📱App: {hostEnvironment.ApplicationName}:{hostEnvironment.EnvironmentName}.

                              ⚠️Exception Type: {exception.GetType().Name}.

                              📝Message: {exception.Message}

                              🔗Path: {httpContext.Request.Path}.

                              👤User Id: {userId}.

                              """;


            Log.Error("[Exception in {App}:{Env}. Type: {ExceptionType}. Path: {Path}. Message: {Message}. UserId: {UserId}]",
                hostEnvironment.ApplicationName,
                hostEnvironment.EnvironmentName,
                exception.GetType().Name,
                httpContext.Request.Path,
                exception.Message,
                userId);


            TelegramLog.LogError(errorMessageForTelegram);

        }

        await result.ExecuteAsync(httpContext);

        return true;
    }

    private static string GetTitle(Exception exception) => exception switch
    {
        ValidationException => "Validation Error",
        AlreadyExistException or ConflictException => "Conflict Error",
        AccessDeniedException => "Access Denied",
        FileNotFoundException or NotFoundException => "Not Found",
        ErrorFromClientException => "Client Error",
        RefreshTokenExpiredException => "Session Timeout",
        DbUpdateException => "Database Error",
        _ => "Server Error"
    };
}
