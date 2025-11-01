using App.Application.Abstractions.Handlers;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Commands.VerifyEmail;
using App.Application.Auth.Queries.Login;
using App.Application.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<LoginQuery, Result<LoginResult>>, LoginHandler>();

        // Commands
        services.AddScoped<ICommandHandler<RegisterCommand, Result<RegisterResult>>, RegisterHandler>();
        services.AddScoped<ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResult>>, VerifyEmailHandler>();
        
        return services;
    }
}