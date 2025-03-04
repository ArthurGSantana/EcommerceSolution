using System;

namespace EcommerceMinified.Domain.ViewModel.Response;

public class Error
{
    public string? Code { get; set; }
    public string? Message { get; set; }

    public Error()
    {
        Code = string.Empty;
    }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
