# 🧩 Minimal API Request Validation with FluentValidation (.NET 10)

This repository demonstrates a clean, modern way to handle request validation in **.NET 10 Minimal APIs** using **FluentValidation** combined with **IEndpointFilter**.

The result is a reusable, composable validation pipeline that keeps your endpoints lightweight and consistent.

---

## 🚀 Features

✅ Clean separation of validation logic  
✅ Reusable `FluentValidationFilter<T>`  
✅ Standardized error responses (`application/problem+json`)  
✅ Compatible with .NET 8 / .NET 9 / .NET 10  
✅ Ready for Swagger/OpenAPI integration  

---

## 🧱 Project Structure

MinimalApiRequestValidation/
├── Filters/
│ └── FluentValidationFilter.cs
├── Models/
│ └── AddUserRequest.cs
├── Validation/
│ └── AddUserRequestValidator.cs
└── Program.cs


---

## 💡 How It Works

### 1️⃣ DTO Model
```
namespace MinimalApiRequestValidation.Models;
public record AddUserRequest(string Name, string Email);
```

### 2️⃣ Validator
```
using FluentValidation;
using MinimalApiRequestValidation.Models;

namespace MinimalApiRequestValidation.Validation;

public class AddUserRequestValidator : AbstractValidator<AddUserRequest>
{
    public AddUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
```

### 3️⃣ Validation Filter
```
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
```

### 4️⃣ Minimal API Setup
```
using FluentValidation;
using MinimalApiRequestValidation.Filters;
using MinimalApiRequestValidation.Models;
using MinimalApiRequestValidation.Validation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidatorsFromAssemblyContaining<AddUserRequestValidator>();

var app = builder.Build();

app.MapPost("/users", (AddUserRequest request) =>
{
    return Results.Ok(new { message = $"User {request.Name} created successfully!" });
})
.AddEndpointFilter<FluentValidationFilter<AddUserRequest>>();

app.Run();
```


### 🧪 Example Request 

 - ✅ Valid Request

```
POST /users
Content-Type: application/json
{
  "name": "Anton Havrylov",
  "email": "anton@example.com"
}
```

Response:
```
{
  "message": "User Anton Havrylov created successfully!"
}
```

 - ❌ Invalid Request
```
POST /users
Content-Type: application/json
{
  "name": "",
  "email": "not-an-email"
}
```

Response:
```
{
  "errors": {
    "Name": ["Name is required."],
    "Email": ["A valid email address is required."]
  }
}
```

### ⚙️ Requirements
```
.NET 10 SDK 
FluentValidation
```

```
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

### 🧠 Why Use Endpoint Filters?

Endpoint Filters were introduced in .NET 7 and allow per-endpoint middleware-like behavior — perfect for request validation, logging, or transformations in Minimal APIs.
Unlike global middleware, filters run after routing and can access strongly-typed parameters directly.

### 📦 Run the Project
```
dotnet run
```

### 🪪 License
```
MIT License — feel free to use and adapt this project in your own APIs.
```
