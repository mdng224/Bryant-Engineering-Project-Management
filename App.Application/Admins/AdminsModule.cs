// App.Application/Admins/AdminsModule.cs
using App.Application.Abstractions;
using App.Application.Admins.Commands.UpdateUser;
using App.Application.Admins.Queries;
using App.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Admins;

public static class AdminsModule
{
    public static IServiceCollection AddAdminsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetUsersQuery, Result<GetUsersResult>>, GetUsersHandler>();

        // Commands
        services.AddScoped<ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>>, UpdateUserHandler>();

        return services;
    }
}
