using Bondstone.Configuration;
using Bondstone.Hosting.Outbox;
using Bondstone.Messaging;
using Bondstone.Modules;
using Bondstone.Persistence.EntityFrameworkCore.Inbox;
using Bondstone.Persistence.EntityFrameworkCore.Outbox;
using Bondstone.Persistence.EntityFrameworkCore.Persistence;
using Bondstone.Persistence.EntityFrameworkCore.Postgres.Persistence;
using Bondstone.Transport.Local.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularTemplate.Tests.Support;
using Shouldly;
using Testcontainers.PostgreSql;

namespace ModularTemplate.Host.Tests.Bondstone;

public sealed class BondstoneDurableMessagingSmokeTests(BondstonePostgreSqlFixture postgreSqlFixture)
    : IClassFixture<BondstonePostgreSqlFixture>
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task DurableCommand_WhenOutboxWorkerRuns_FlowsThroughLocalTransportAndConsumerInbox()
    {
        var recorder = new SmokeRecorder();
        ServiceProvider services = CreateServices(postgreSqlFixture.ConnectionString, recorder);
        await using ServiceProvider asyncServices = services;

        await CreateBondstoneTablesAsync(asyncServices);

        using AsyncServiceScope sendScope = asyncServices.CreateAsyncScope();
        IModuleCommandExecutor executor =
            sendScope.ServiceProvider.GetRequiredService<IModuleCommandExecutor>();

        Guid commandId = Guid.NewGuid();
        await executor.ExecuteAsync(
            "producer",
            new ProduceSmokeCommand(commandId),
            CancellationToken.None);

        DurableMessageEnvelope stagedEnvelope = await ReadStagedEnvelopeAsync(asyncServices);
        stagedEnvelope.TargetModule.ShouldBe("consumer");
        stagedEnvelope.MessageTypeName.ShouldBe("smoke.accept.v1");

        IHostedService outboxWorker = asyncServices.GetServices<IHostedService>().Single();
        await outboxWorker.StartAsync(CancellationToken.None);
        try
        {
            await recorder.WaitForAsync(commandId);
        }
        finally
        {
            await outboxWorker.StopAsync(CancellationToken.None);
        }

        recorder.HandledCommandIds.ShouldBe([commandId]);

        using AsyncServiceScope duplicateScope = asyncServices.CreateAsyncScope();
        var duplicateCommand = new AcceptSmokeCommand(Guid.NewGuid());
        DurableMessageEnvelope duplicateEnvelope = CreateEnvelope(
            duplicateScope.ServiceProvider,
            duplicateCommand);
        IModuleCommandReceivePipeline receivePipeline =
            duplicateScope.ServiceProvider.GetRequiredService<IModuleCommandReceivePipeline>();

        await receivePipeline.HandleOnceAsync(duplicateEnvelope, CancellationToken.None);
        await receivePipeline.HandleOnceAsync(duplicateEnvelope, CancellationToken.None);

        recorder.HandledCommandIds.ShouldBe([commandId, duplicateCommand.CommandId]);
        (await CountConsumerInboxRowsAsync(asyncServices)).ShouldBe(1);
    }

    private static ServiceProvider CreateServices(
        string connectionString,
        SmokeRecorder recorder)
    {
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddDebug());
        services.AddSingleton(recorder);
        services.AddBondstone(bondstone =>
        {
            bondstone.Module(
                "producer",
                module =>
                {
                    module
                        .UseDurableMessaging()
                        .UsePostgreSqlPersistence<ProducerSmokeDbContext>(
                            connectionString,
                            schema: "producer");
                    module.Commands.RegisterHandler<ProduceSmokeCommand, ProduceSmokeCommandHandler>(
                        "smoke.produce.v1");
                });
            bondstone.Module(
                "consumer",
                module =>
                {
                    module
                        .UseDurableMessaging()
                        .UsePostgreSqlPersistence<ConsumerSmokeDbContext>(
                            connectionString,
                            schema: "consumer");
                    module.Commands.RegisterHandler<AcceptSmokeCommand, AcceptSmokeCommandHandler>();
                });
            bondstone.UseLocalTransport(transport => transport.UseModuleQueueConvention());
            bondstone.Outbox
                .UseDurableDispatcher()
                .UseWorker();
        });

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static async Task CreateBondstoneTablesAsync(ServiceProvider services)
    {
        using AsyncServiceScope scope = services.CreateAsyncScope();
        var producer = scope.ServiceProvider.GetRequiredService<ProducerSmokeDbContext>();
        var consumer = scope.ServiceProvider.GetRequiredService<ConsumerSmokeDbContext>();

        await producer.Database.EnsureDeletedAsync(CancellationToken.None);
        await producer.Database.EnsureCreatedAsync(CancellationToken.None);
        await consumer.Database.ExecuteSqlRawAsync(
            consumer.Database.GenerateCreateScript(),
            CancellationToken.None);
    }

    private static async Task<DurableMessageEnvelope> ReadStagedEnvelopeAsync(ServiceProvider services)
    {
        using AsyncServiceScope scope = services.CreateAsyncScope();
        var producer = scope.ServiceProvider.GetRequiredService<ProducerSmokeDbContext>();
        OutboxMessageEntity outboxMessage = await producer.Set<OutboxMessageEntity>()
            .SingleAsync(CancellationToken.None);

        return outboxMessage.ToRecord().Envelope;
    }

    private static DurableMessageEnvelope CreateEnvelope(
        IServiceProvider services,
        AcceptSmokeCommand command)
    {
        IDurablePayloadSerializer serializer =
            services.GetRequiredService<IDurablePayloadSerializer>();

        return new DurableMessageEnvelope(
            Guid.NewGuid(),
            MessageKind.Command,
            "smoke.accept.v1",
            "producer",
            "consumer",
            serializer.Serialize(command),
            DateTimeOffset.UtcNow);
    }

    private static async Task<int> CountConsumerInboxRowsAsync(ServiceProvider services)
    {
        using AsyncServiceScope scope = services.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<ConsumerSmokeDbContext>();

        return await consumer.Set<InboxMessageEntity>().CountAsync(CancellationToken.None);
    }

    private sealed class ProducerSmokeDbContext(DbContextOptions<ProducerSmokeDbContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyBondstonePersistence("producer");
        }
    }

    private sealed class ConsumerSmokeDbContext(DbContextOptions<ConsumerSmokeDbContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyBondstonePersistence("consumer");
        }
    }

    private sealed record ProduceSmokeCommand(Guid CommandId) : ICommand;

    [DurableCommandIdentity("smoke.accept.v1")]
    private sealed record AcceptSmokeCommand(Guid CommandId) : IDurableCommand;

    private sealed class ProduceSmokeCommandHandler(IDurableCommandSender durableCommandSender)
        : ICommandHandler<ProduceSmokeCommand>
    {
        public async ValueTask HandleAsync(
            ProduceSmokeCommand command,
            CancellationToken ct = default)
        {
            await durableCommandSender.SendAsync(
                new AcceptSmokeCommand(command.CommandId),
                "consumer",
                ct);
        }
    }

    private sealed class AcceptSmokeCommandHandler(SmokeRecorder recorder)
        : ICommandHandler<AcceptSmokeCommand>
    {
        public ValueTask HandleAsync(
            AcceptSmokeCommand command,
            CancellationToken ct = default)
        {
            recorder.Record(command.CommandId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class SmokeRecorder
    {
        private readonly TaskCompletionSource<Guid> _handled = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<Guid> _handledCommandIds = [];

        public IReadOnlyCollection<Guid> HandledCommandIds
        {
            get
            {
                lock (_handledCommandIds)
                {
                    return [.. _handledCommandIds];
                }
            }
        }

        public void Record(Guid commandId)
        {
            lock (_handledCommandIds)
            {
                _handledCommandIds.Add(commandId);
            }

            _handled.TrySetResult(commandId);
        }

        public async Task WaitForAsync(Guid commandId)
        {
            Task completed = await Task.WhenAny(
                _handled.Task,
                Task.Delay(TimeSpan.FromSeconds(10)));
            completed.ShouldBe(_handled.Task);
            _handled.Task.Result.ShouldBe(commandId);
        }
    }
}

public sealed class BondstonePostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public BondstonePostgreSqlFixture()
    {
        ContainerRuntimeDefaults.Apply();

        _container = new PostgreSqlBuilder("docker.io/library/postgres:17-alpine")
            .WithDatabase("modular_template_bondstone_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }
}
