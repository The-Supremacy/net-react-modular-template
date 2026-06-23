using Bondstone.Modules;
using Bondstone.Persistence.EntityFrameworkCore.Persistence;
using Bondstone.Persistence.EntityFrameworkCore.Postgres.Persistence;
using ModularTemplate.Identity.Infrastructure.Persistence;

namespace ModularTemplate.Identity.Infrastructure;

public sealed class IdentityBondstoneModule(string connectionString) : IBondstoneModule
{
    public string Name => IdentityDbContext.ModuleName;

    public void Configure(BondstoneModuleBuilder module)
    {
        module
            .UseDurableMessaging()
            .UsePostgreSqlPersistence<IdentityDbContext>(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", IdentityDbContext.ModuleName),
                IdentityDbContext.ModuleName)
            .UseEntityFrameworkCoreDomainEventPersistence();
    }
}
