using FluentValidation.Results;

namespace MyService.Application.Extensions;

public static class FluentValidationExtensions
{
  /// <summary>
  /// Converts a collection of validation errors to a dictionary.
  /// </summary>
  /// <param name="validationErrors">The collection of validation errors.</param>
  /// <returns>Returns a dictionary where the keys are property names and the values are arrays of error messages.</returns>
  public static IDictionary<string, string[]> ToDictionary(this IEnumerable<ValidationFailure> validationErrors)
  {
    return validationErrors
      .GroupBy(x => x.PropertyName)
      .ToDictionary(
        g => g.Key,
        g => g.Select(x => x.ErrorMessage).ToArray()
      );
  }
}