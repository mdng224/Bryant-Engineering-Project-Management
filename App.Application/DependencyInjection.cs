using App.Application.Abstractions;
using App.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // application-layer services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}