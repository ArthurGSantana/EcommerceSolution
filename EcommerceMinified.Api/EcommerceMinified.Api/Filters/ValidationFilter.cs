using System;
using System.Text.Json;
using EcommerceMinified.Domain.ViewModel.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcommerceMinified.Api.Filters;

public class ValidationFilter(ILogger<ValidationFilter> _logger) : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context) { }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Where(v => v.Value?.Errors.Count > 0)
                .SelectMany(v =>
                    v.Value!.Errors.Select(e => new Error() { Code = v.Key.TrimStart('$', '.'), Message = e.ErrorMessage }))
                .ToList();

            var dtoName = context.ActionArguments.FirstOrDefault(arg => arg.Value != null).Value?.GetType().Name;
            _logger.LogError($"Validating {dtoName} in {context.ActionDescriptor.DisplayName}");
            _logger.LogError(new Exception(JsonSerializer.Serialize(errors)), $"Validation error in {context.ActionDescriptor.DisplayName}");

            context.Result = new UnprocessableEntityObjectResult(new ErrorViewModel(errors));
        }
    }
}
