using Bondstone.Persistence.EntityFrameworkCore.Persistence;
using Microsoft.EntityFrameworkCore;
using ModularTemplate.Identity.Access;
using ModularTemplate.Identity.Users;

namespace ModularTemplate.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options)
{
    public const string ModuleName = "identity";

    public DbSet<LocalUser> LocalUsers => Set<LocalUser>();

    public DbSet<ApplicationAccess> ApplicationAccess => Set<ApplicationAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ModuleName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        modelBuilder.ApplyBondstonePersistence(ModuleName);
        modelBuilder.ApplyBondstoneDomainEvents(ModuleName);
    }
}
