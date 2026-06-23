using Bondstone.Modules;
using Bondstone.Persistence.EntityFrameworkCore.Persistence;
using Bondstone.Persistence.EntityFrameworkCore.Postgres.Persistence;
using ModularTemplate.Products.Infrastructure.Persistence;

namespace ModularTemplate.Products.Infrastructure;

public sealed class ProductsBondstoneModule(string connectionString) : IBondstoneModule
{
    public string Name => ProductsDbContext.ModuleName;

    public void Configure(BondstoneModuleBuilder module)
    {
        module
            .UseDurableMessaging()
            .UsePostgreSqlPersistence<ProductsDbContext>(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", ProductsDbContext.ModuleName),
                ProductsDbContext.ModuleName)
            .UseEntityFrameworkCoreDomainEventPersistence();
    }
}
