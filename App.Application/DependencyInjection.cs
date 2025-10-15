using App.Application.Admins;
using App.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Modules
        services.AddAuthApplication();
        services.AddAdminsApplication();

        return services;
    }
}