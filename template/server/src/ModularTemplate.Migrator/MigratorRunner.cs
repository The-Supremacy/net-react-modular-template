using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularTemplate.Identity.Access;

namespace ModularTemplate.Migrator;

public static class MigratorRunner
{
    public static async Task<int> RunAsync(
        string[] args,
        IConfiguration configuration,
        IServiceProvider services,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        if (!MigratorCommand.TryParse(args, out MigratorCommand command, out string? parseError))
        {
            await error.WriteLineAsync(parseError);
            return 2;
        }

        await using AsyncServiceScope scope = services.CreateAsyncScope();

        if (command.MigrationScope is MigratorMigrationScope.All or MigratorMigrationScope.Modules)
        {
            await scope.ServiceProvider.GetRequiredService<Identity.Infrastructure.Persistence.IdentityDbContext>()
                .Database.MigrateAsync(cancellationToken);
            await scope.ServiceProvider.GetRequiredService<Products.Infrastructure.Persistence.ProductsDbContext>()
                .Database.MigrateAsync(cancellationToken);
        }

        if (command.MigrationScope is MigratorMigrationScope.Module)
        {
            switch (command.ModuleName)
            {
                case "identity":
                    await scope.ServiceProvider.GetRequiredService<Identity.Infrastructure.Persistence.IdentityDbContext>()
                        .Database.MigrateAsync(cancellationToken);
                    break;
                case "products":
                    await scope.ServiceProvider.GetRequiredService<Products.Infrastructure.Persistence.ProductsDbContext>()
                        .Database.MigrateAsync(cancellationToken);
                    break;
                default:
                    await error.WriteLineAsync($"Unknown module '{command.ModuleName}'.");
                    return 2;
            }
        }

        string? configurationError = null;
        InitialAdminOptions? initialAdmin = command.InitialAdmin
            ?? (command.UseConfiguredInitialAdmin
                ? ReadConfiguredInitialAdmin(configuration, out configurationError)
                : null);
        if (configurationError is not null)
        {
            await error.WriteLineAsync(configurationError);
            return 2;
        }

        if (initialAdmin is null)
        {
            return 0;
        }

        GrantInitialAdminAccessCommandHandler handler =
            scope.ServiceProvider.GetRequiredService<GrantInitialAdminAccessCommandHandler>();
        GrantInitialAdminAccessResult result = await handler.HandleAsync(
            new GrantInitialAdminAccessCommand(
                initialAdmin.Provider,
                initialAdmin.Subject,
                initialAdmin.Force),
            cancellationToken);

        await output.WriteLineAsync($"Initial admin setup: {result}.");
        return result == GrantInitialAdminAccessResult.RevokedRequiresForce ? 1 : 0;
    }

    private static InitialAdminOptions? ReadConfiguredInitialAdmin(
        IConfiguration configuration,
        out string? error)
    {
        error = null;

        InitialAdminOptions options = configuration
            .GetSection("Identity:InitialAdmin")
            .Get<InitialAdminOptions>()
            ?? new InitialAdminOptions();

        bool hasProvider = !string.IsNullOrWhiteSpace(options.Provider);
        bool hasSubject = !string.IsNullOrWhiteSpace(options.Subject);
        if (!hasProvider && !hasSubject)
        {
            return null;
        }

        if (!hasProvider || !hasSubject)
        {
            error = "Identity:InitialAdmin requires both Provider and Subject when either value is configured.";
            return null;
        }

        return options;
    }
}
