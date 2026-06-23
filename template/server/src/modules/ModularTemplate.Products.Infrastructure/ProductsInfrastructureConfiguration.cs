using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModularTemplate.Products;
using ModularTemplate.Products.Contracts.Products;
using ModularTemplate.Products.Products;
using ModularTemplate.Products.Infrastructure.Persistence;

namespace ModularTemplate.Products.Infrastructure;

public static class ProductsInfrastructureConfiguration
{
    public static IServiceCollection AddProductsModule(this IServiceCollection services)
    {
        services.AddProductsApplicationServices();
        services.AddProductsInfrastructure();

        return services;
    }

    public static IEndpointRouteBuilder MapProductsModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapProductsEndpoints();

        return endpoints;
    }

    public static IServiceCollection AddProductsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductsQueries, ProductsQueries>();

        return services;
    }
}
