using System;
using EcommerceMinified.Domain.Enum;

namespace EcommerceMinified.Domain.ViewModel.Response;

public class ErrorViewModel
{
    public List<Error> Errors { get; private set; }

    public ErrorViewModel()
    {
        Errors = new List<Error>();
    }

    public ErrorViewModel(List<Error> errors)
    {
        Errors = errors;
    }

    public ErrorViewModel(Error error)
    {
        Errors = new List<Error> { error };
    }

    public ErrorViewModel(string code, string? message = "")
    {

        Errors = new List<Error>()
        {
            new Error()
            {
                Code = code,
                Message = message
            }
        };

    }

    public ErrorViewModel(List<string> errors)
    {
        Errors = errors
                    .Select(e => new Error() { Code = System.Enum.GetName(ErrorCodeEnum.ValidationError) ?? "", Message = e })
                    .ToList();
}
}
