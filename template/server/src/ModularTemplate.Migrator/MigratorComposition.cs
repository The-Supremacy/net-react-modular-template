using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularTemplate.Identity.Infrastructure;
using ModularTemplate.Products.Infrastructure;

namespace ModularTemplate.Migrator;

public static class MigratorComposition
{
    public static IHostApplicationBuilder AddMigratorComposition(this IHostApplicationBuilder builder)
    {
        builder.Services.AddIdentityModule();
        builder.Services.AddProductsModule();
        builder.AddBondstoneRuntime();

        return builder;
    }
}
