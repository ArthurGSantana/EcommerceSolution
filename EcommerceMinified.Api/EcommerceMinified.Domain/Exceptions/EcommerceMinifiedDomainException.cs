using System;
using System.Net;
using EcommerceMinified.Domain.Enum;

namespace EcommerceMinified.Domain.Exceptions;

public class EcommerceMinifiedDomainException : Exception
{
    public ErrorCodeEnum ErrorCode { get; set; }
    public HttpStatusCode? StatusCode { get; set; }

    public EcommerceMinifiedDomainException(string message, ErrorCodeEnum errorCode, HttpStatusCode? statusCode = null) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public EcommerceMinifiedDomainException(string message, Exception innerException, ErrorCodeEnum errorCode, HttpStatusCode? statusCode = null) : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
