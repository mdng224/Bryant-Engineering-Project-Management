using App.Application.Abstractions.Handlers;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Commands.VerifyEmail;
using App.Application.Auth.Queries.Login;
using App.Application.Common.Results;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        // Commands
        services.AddScoped<ICommandHandler<RegisterCommand, Result<RegisterResult>>, RegisterHandler>();
        services.AddScoped<ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResult>>, VerifyEmailHandler>();
        
        // Queries
        services.AddScoped<IQueryHandler<LoginQuery, Result<LoginResult>>, LoginHandler>();
        
        // Validators
        services.AddScoped<IValidator<LoginQuery>, LoginQueryValidator>();
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        
        return services;
    }
}