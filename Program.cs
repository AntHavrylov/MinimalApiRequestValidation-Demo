using FluentValidation;
using MinimalApiRequestValidation.Filters;
using MinimalApiRequestValidation.Models;
using MinimalApiRequestValidation.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<AddUserRequestValidator>();

var app = builder.Build();


app.MapPost("/users", (AddUserRequest request) => 
{
    return Results.Ok(new { mewssage = $"User {request.Name} created successfully!" });
})
.AddEndpointFilter<FluentValidationFilter<AddUserRequest>>();

app.Run();
