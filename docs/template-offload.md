# Template Archive Note

This note records the final disposition of this template factory. The repository
is parked as portfolio material rather than developed into the preferred
initializer for future products.

## Archive Decision

The template experiment proved useful architecture and bootstrap ideas, but it
did not deliver enough value to justify continuing it as a standing product
factory. Future projects should be structured directly for the project at hand
instead of starting from this repository by default.

Bondstone remains consumed as packages in this final snapshot because that is
the clearest archive state: reusable modular-boundary behavior lives outside the
template payload, and the generated product no longer vendors a second copy of
Bondstone implementation code.

## Snapshot Role

This repository now serves as a deterministic record of the final template
shape:

- generated repository layout and bootstrap naming behavior;
- Aspire local platform setup for PostgreSQL, Redis, Keycloak, Host, Migrator,
  Admin, and Web resources;
- module-owned EF Core DbContexts and baseline migrations;
- package-based Bondstone 1.3 composition for module boundaries, local
  transport, durable persistence, and hosted outbox dispatch;
- generated-product docs, governance, and agent instructions.

## If Work Resumes

Treat resumed work as a fresh product decision, not as automatic continuation
of this template. Re-evaluate the runtime stack, messaging library, frontend
shape, local platform, CI, and bootstrap needs against the concrete product.
Wolverine or direct project-specific structure may be a better starting point
than reviving this factory.
