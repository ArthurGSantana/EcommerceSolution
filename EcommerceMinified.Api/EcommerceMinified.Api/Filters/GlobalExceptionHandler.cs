using EcommerceMinified.Api.Extensions;
using EcommerceMinified.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;

namespace EcommerceMinified.Api.Filters;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogError(exception, "Exception occurred during request processing");

        var problemDetails = CreateProblemDetails(httpContext, exception);

        ConfigureResponseBasedOnException(httpContext, exception, problemDetails);

        // Adicionar detalhes de debug em ambientes não produtivos
        if (!_env.IsProduction())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var serviceName = GetServiceName(exception);

        return new ProblemDetails
        {
            Detail = exception.Message,
            Instance = context.Request.Path + context.Request.QueryString,
            Extensions =
            {
                ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier,
                ["innerException"] = exception.InnerException?.Message
            }
        };
    }

    private void ConfigureResponseBasedOnException(
        HttpContext context,
        Exception exception,
        ProblemDetails problemDetails)
    {
        var serviceName = GetServiceName(exception);

        switch (exception)
        {
            case EcommerceMinifiedDomainException domainEx:
                context.Response.StatusCode = (int)(domainEx.StatusCode ?? HttpStatusCode.BadRequest);
                problemDetails.Status = context.Response.StatusCode;
                problemDetails.Detail = domainEx.Message;
                problemDetails.Title = $"{serviceName}/{Enum.GetName(domainEx.ErrorCode)?.ToSnakeCase().ToLower()}";
                break;

            case ArgumentException:
            case ValidationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = $"{serviceName}/validation_error";
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = $"{serviceName}/unauthorized";
                break;

            case System.Collections.Generic.KeyNotFoundException:
            case FileNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = $"{serviceName}/not_found";
                break;
            case FluentValidation.ValidationException valEx: //FluentValidation
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = $"{serviceName}/validation_error";
                // Adiciona detalhes de validação ao problema
                problemDetails.Extensions["errors"] = valEx.Errors.Select(e => new
                {
                    e.PropertyName,
                    e.ErrorMessage
                });
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = $"{serviceName}/internal_server_error";

                if (_env.IsProduction())
                {
                    _logger.LogCritical(exception, "Unhandled exception");
                    problemDetails.Detail = "An internal error has occurred. Please try again later.";
                }
                break;
        }
    }

    private string GetServiceName(Exception exception)
    {
        if (string.IsNullOrEmpty(exception.Source))
            return "EcommerceMinified";

        return exception.Source.Split('.', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "EcommerceMinified";
    }
}