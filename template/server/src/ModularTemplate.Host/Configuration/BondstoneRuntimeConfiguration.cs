using Bondstone.Configuration;
using Bondstone.Hosting.Outbox;
using Bondstone.Modules;
using Bondstone.Transport.Local.Outbox;
using ModularTemplate.Identity.Infrastructure;
using ModularTemplate.Products.Infrastructure;

namespace ModularTemplate.Host.Configuration;

internal static class BondstoneRuntimeConfiguration
{
    public static IHostApplicationBuilder AddBondstoneRuntime(this IHostApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString("modular-template-host")
            ?? throw new InvalidOperationException(
                "Connection string 'modular-template-host' is required.");

        builder.Services.AddBondstone(bondstone =>
        {
            bondstone.AddModule(new IdentityBondstoneModule(connectionString));
            bondstone.AddModule(new ProductsBondstoneModule(connectionString));
            bondstone.UseLocalTransport(transport => transport.UseModuleQueueConvention());
            bondstone.Outbox
                .UseDurableDispatcher()
                .UseWorker();
        });

        return builder;
    }
}
