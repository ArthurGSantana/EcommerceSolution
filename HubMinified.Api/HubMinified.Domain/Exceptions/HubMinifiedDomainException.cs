using System;
using System.Net;
using HubMinified.Domain.Enum;

namespace HubMinified.Domain.Exceptions;

public class HubMinifiedDomainException : Exception
{
    public ErrorCodeEnum ErrorCode { get; set; }
    public HttpStatusCode? StatusCode { get; set; }

    public HubMinifiedDomainException(string message, ErrorCodeEnum errorCode, HttpStatusCode? statusCode = null) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public HubMinifiedDomainException(string message, Exception innerException, ErrorCodeEnum errorCode, HttpStatusCode? statusCode = null) : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
