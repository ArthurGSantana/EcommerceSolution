using EcommerceMinified.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Net;

namespace Desafio4.Api.Filters;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    : IExceptionHandler
{
    private static readonly char[] Separator = [',', '.'];

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var serviceName = exception.Source?.Split(Separator, StringSplitOptions.RemoveEmptyEntries)[0] ?? "ServiceNotSpecified";
        const int statusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = $"{serviceName}/{ReasonPhrases.GetReasonPhrase(httpContext.Response.StatusCode)}",
            Detail = exception.Message,
            Extensions = { ["innerException"] = exception.InnerException?.Message },
            Instance = httpContext.Request.Path + httpContext.Request.QueryString
        };

        if (env.IsProduction() && httpContext.Response.StatusCode == StatusCodes.Status500InternalServerError)
        {
            problemDetails.Detail = "An internal error occurred. Please try again later.";
        }

        if (exception is EcommerceMinifiedDomainException exp)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            problemDetails.Status = (int)HttpStatusCode.BadRequest;
            problemDetails.Detail = exp.Message;
            problemDetails.Title = $"{serviceName}/{Enum.GetName(exp.ErrorCode)?.ToSnakeCase().ToLower()}";

            if (exp.StatusCode.HasValue)
            {
                httpContext.Response.StatusCode = (int)exp.StatusCode;
                problemDetails.Status = (int)exp.StatusCode;
            }
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
}