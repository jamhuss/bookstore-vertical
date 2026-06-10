using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookStore.Helpers;

/// <summary>
/// Shared helper that maps FluentValidation results onto MVC ModelState so any slice
/// can return a standard ValidationProblem (RFC 7807) response with a single call.
/// </summary>
public static class ValidationExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
