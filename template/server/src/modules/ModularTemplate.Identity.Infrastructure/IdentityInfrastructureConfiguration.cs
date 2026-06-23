using Microsoft.Extensions.DependencyInjection;
using ModularTemplate.Identity.Contracts.Authorization;
using ModularTemplate.Identity.CurrentUser;
using ModularTemplate.Identity;
using ModularTemplate.Identity.Infrastructure.Persistence;
using ModularTemplate.Identity.Users;
using ModularTemplate.Identity.Access;

namespace ModularTemplate.Identity.Infrastructure;

public static class IdentityInfrastructureConfiguration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddIdentityApplicationServices();
        services.AddIdentityInfrastructure();

        return services;
    }

    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocalUserRepository, LocalUserRepository>();
        services.AddScoped<IApplicationAccessRepository, ApplicationAccessRepository>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        return services;
    }
}
