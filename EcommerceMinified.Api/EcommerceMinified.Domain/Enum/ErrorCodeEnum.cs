using System;

namespace EcommerceMinified.Domain.Enum;

public enum ErrorCodeEnum
{
    NotFound = 404,
    BadRequest = 400,
    AlreadyExists = 409,
    InternalServerError = 500,
    ValidationError = 422
}
