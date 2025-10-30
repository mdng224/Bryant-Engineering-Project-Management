// App.Application/Users/UsersModule.cs

using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Users.Commands.UpdateUser;
using App.Application.Users.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>, GetUsersHandler>();

        // Commands
        services.AddScoped<ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>>, UpdateUserHandler>();

        return services;
    }
}
