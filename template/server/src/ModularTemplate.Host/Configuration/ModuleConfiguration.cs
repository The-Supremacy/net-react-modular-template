using ModularTemplate.Host.Authorization;
using ModularTemplate.Host.Features.CurrentUser;
using ModularTemplate.Identity.CurrentUser;
using ModularTemplate.Identity.Infrastructure;
using ModularTemplate.Products.Infrastructure;

namespace ModularTemplate.Host.Configuration;

public static class ModuleConfiguration
{
    public static IServiceCollection AddModularTemplateModules(this IServiceCollection services)
    {
        services.AddIdentityModule();
        services.AddProductsModule();
        services.AddApplicationAccessAuthorization();

        return services;
    }

    public static IEndpointRouteBuilder MapModularTemplateModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCurrentUserEndpoint();
        endpoints.MapProductsModule();

        return endpoints;
    }
}
