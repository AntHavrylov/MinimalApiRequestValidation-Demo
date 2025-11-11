
using FluentValidation;

namespace MinimalApiRequestValidation.Filters;

public class FluentValidationFilter<T> : IEndpointFilter
{
    private readonly IValidator<T>? _validator;

    public FluentValidationFilter(IValidator<T>? validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var dto = context.Arguments.OfType<T>().FirstOrDefault();
        if (dto is null)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["Body"] = new[] { "Request body is missing or invalid." }
            };
            return Results.ValidationProblem(errors);
        }

        if (_validator is not null)
        {
            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                var errorsDict = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
                return Results.ValidationProblem(errorsDict);
            }
        }

        return await next(context);

    }
}
