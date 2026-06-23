using Bondstone.Persistence.EntityFrameworkCore.Persistence;
using Microsoft.EntityFrameworkCore;
using ModularTemplate.Products.Products;

namespace ModularTemplate.Products.Infrastructure.Persistence;

public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options)
    : DbContext(options)
{
    public const string ModuleName = "products";

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ModuleName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);
        modelBuilder.ApplyBondstonePersistence(ModuleName);
        modelBuilder.ApplyBondstoneDomainEvents(ModuleName);
    }
}
