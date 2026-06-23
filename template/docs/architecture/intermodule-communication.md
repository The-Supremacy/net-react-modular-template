# Intermodule Communication

This template keeps module boundaries explicit. A module owns its state, its
`DbContext`, its schema, and its application behavior. Other modules interact
through provider-neutral contracts or durable messages, not through EF sets,
tables, domain entities, or infrastructure types.

## Bondstone Ownership

Bondstone is consumed as NuGet packages. The template must not copy or
reimplement Bondstone infrastructure.

Bondstone owns:

- module registration through `AddBondstone` and module-owned
  `IBondstoneModule` implementations;
- durable command identities, command receive pipelines, outbox writing,
  inbox duplicate detection, operation state, and transport topology
  validation;
- EF Core outbox, inbox, operation-state, and domain-event mappings through
  `ApplyBondstonePersistence()` and `ApplyBondstoneDomainEvents()`;
- PostgreSQL persistence helpers through
  `UsePostgreSqlPersistence<TDbContext>()`;
- hosted outbox dispatch through `UseDurableDispatcher().UseWorker()`;
- local transport routing through `Bondstone.Transport.Local`.

The template owns product/application commands, handlers, repositories,
queries, authorization, auth/session behavior, module `DbContext`s, migrations,
and thin composition that calls Bondstone package APIs.

## Choosing A Pattern

Use the smallest communication pattern that matches the consistency need.

Same-module behavior may use module-local services or Bondstone command
handlers. Result-returning application work should stay as application logic
outside durable messaging. Durable messaging is for asynchronous handoff across
a reliability boundary, not for ordinary in-process function calls.

Cross-module synchronous reads may use query contracts from the target
module's `.Contracts` project when the caller truly needs immediate read-side
state. Contracts return DTOs or read models, not EF entities, aggregates,
provider SDK types, `ClaimsPrincipal`, or Host HTTP concepts.

Cross-module writes and eventual work should use durable commands or
integration events. Durable commands are send-and-forget: the sender receives
an acceptance result and optional durable operation id, while completion is
observed later through state, read models, query contracts, or follow-up
events.

## Durable Lifecycle

Durable messages follow this shape:

1. source module application code runs inside a Bondstone module pipeline;
2. `IDurableCommandSender` writes a source-module outbox row through Bondstone;
3. the package-hosted outbox worker claims pending rows;
4. `Bondstone.Transport.Local` routes the message to the target module queue;
5. the target command receive pipeline deserializes and invokes the registered
   handler;
6. Bondstone inbox persistence suppresses duplicate receive-pipeline delivery
   for the same message id and handler identity.

The generated template uses local transport for the first-version local/dev
topology. It has no separate transport schema, broker, or `migrate transport`
step. Products that need a broker should adopt a supported Bondstone transport
package and add product-owned provisioning, configuration, and verification.

The default local transport routes are explicit in Host and Migrator
composition:

- `identity` routes to the `identity` queue and accepts the `identity` module;
- `products` routes to the `products` queue and accepts the `products` module.

## Persistence

Each module Infrastructure project owns its EF Core `DbContext`, schema, and
baseline `InitialCreate` migration. Module `DbContext`s inherit directly from
EF Core `DbContext` and apply package mappings:

```csharp
modelBuilder.ApplyBondstonePersistence("identity");
modelBuilder.ApplyBondstoneDomainEvents("identity");
```

Module-owned `IBondstoneModule` implementations opt into durable messaging and
PostgreSQL persistence:

```csharp
module
    .UseDurableMessaging()
    .UsePostgreSqlPersistence<IdentityDbContext>(
        connectionString,
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identity"),
        "identity")
    .UseEntityFrameworkCoreDomainEventPersistence();
```

Outbox, inbox, and domain-event tables are package-owned schema. Generated
migrations should reflect package entity names and column shapes; do not keep
template-owned inbox/outbox entities, dispatchers, locks, retry policies,
maintenance APIs, or EF mapping helpers.

## Message Contracts

Messages must use stable identities, not CLR type names. Durable commands use
`DurableCommandIdentityAttribute`. Domain events that are persisted by the
domain-events capability use `DomainEventIdentityAttribute`.

Payloads become durable contracts once written to an outbox row. Prefer
append-compatible changes. Introduce a new message identity for breaking
payload changes after retained rows and in-flight messages have drained.

## Adding A Module

When adding a module:

1. add `{Module}`, `{Module}.Contracts`, and `{Module}.Infrastructure`
   projects;
2. put domain and application behavior in `{Module}`;
3. put provider-neutral contracts and DTOs in `{Module}.Contracts`;
4. put EF Core, repositories, query implementations, and adapters in
   `{Module}.Infrastructure`;
5. create a module `DbContext` and apply Bondstone persistence/domain-event
   mappings in `OnModelCreating`;
6. create an `IBondstoneModule` in module Infrastructure that calls
   `UseDurableMessaging()` and `UsePostgreSqlPersistence<TDbContext>()`;
7. add the module to Host and Migrator `AddBondstone` composition;
8. add explicit local transport route/queue bindings or configure another
   supported Bondstone transport;
9. add module docs, migrations, and focused tests.

## Adding A Durable Command

1. Define a command that implements `IDurableCommand`.
2. Add `DurableCommandIdentityAttribute`.
3. Register a target-module handler with `module.Commands.RegisterHandler`.
4. Send it through `IDurableCommandSender` from source-module application code
   running inside a Bondstone module pipeline.
5. Observe completion through state, query contracts, or follow-up events.

Generated products should add tests for real communication workflows as those
workflows appear. The template includes a focused smoke test proving
package-based outbox staging, hosted dispatch, local transport delivery,
receive-pipeline handling, and inbox idempotency.
