// App.Application/Auth/AuthModule.cs
using App.Application.Abstractions;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Queries.Login;
using App.Application.Common;
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

        return services;
    }
}