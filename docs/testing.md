# Archive Validation Plan

Use this checklist to prove the archived template snapshot still bootstraps and
verifies. It is not a release plan and does not include npm publication steps.

## 1. Confirm Factory Baseline

From the factory root:

```sh
node -v
pnpm exec node -v
pnpm verify
pnpm template:verify:full
```

Expected:

- `node -v` reports Node 24.15.0 or newer.
- `pnpm exec node -v` reports Node 24.15.0 or newer.
- `pnpm verify` passes, including root script checks and focused bootstrap
  verification.
- `pnpm template:verify:full` passes against Docker or Podman when full
  generated-product validation is needed.

For rootless Podman on Linux, set:

```sh
systemctl --user enable --now podman.socket

export DOCKER_HOST="unix://${XDG_RUNTIME_DIR}/podman/podman.sock"
export TESTCONTAINERS_RYUK_DISABLED=true
export ASPIRE_CONTAINER_RUNTIME=podman
```

## 2. Run The Template Payload Locally

From `template/`:

```sh
pnpm install --frozen-lockfile
dotnet tool restore
dotnet restore ModularTemplate.slnx
```

Start Aspire:

```sh
ASPIRE_CONTAINER_RUNTIME=${ASPIRE_CONTAINER_RUNTIME:-docker} \
  aspire start --apphost orchestration/ModularTemplate.Orchestration/ModularTemplate.Orchestration.csproj --isolated
```

Expected:

- PostgreSQL, Redis, Keycloak, Migrator, Host, Admin, and Web resources start.
- Migrator applies the checked-in baseline migration and grants the local
  Keycloak smoke admin through `Identity:InitialAdmin`.
- Admin and Web apps load.
- Browser auth smoke works with:
  - `admin@example.test` / `Password123!`
  - `user@example.test` / `Password123!`
- Logout returns the app to an unauthenticated state.

## 3. Test The Local Packed CLI

From the factory root:

```sh
mkdir -p /tmp/dotnet-modular-react-template-pack
PACKED_TEMPLATE="$(pnpm pack --pack-destination /tmp/dotnet-modular-react-template-pack | tail -n 1)"

pnpm dlx "${PACKED_TEMPLATE}" \
  -- --product-name "Smoke Desk" --output /tmp/smoke-desk
```

In `/tmp/smoke-desk`:

```sh
pnpm install --frozen-lockfile
pnpm verify
```

Then repeat the Aspire startup from section 2, using the renamed solution and
project paths generated for `Smoke Desk`.

Expected:

- The generated repository contains product files at its root, not under
  `template/`.
- Names are rewritten to `SmokeDesk`, `smoke-desk`, and `smoke_desk`.
- Product CI, Husky hooks, VS Code/Aspire config, and docs are present.
- The baseline `InitialCreate` EF migrations are renamed for the generated
  product and present under its module Infrastructure projects.
- OpenSpec scaffolding is absent from the generated payload.
- Factory-only docs and packaging files are absent.
- The generated product starts locally from Aspire.

## 4. Archive Decision

The snapshot is archive-ready when:

- Factory verification passes.
- Template payload runs locally from Aspire when full smoke validation is
  needed.
- Local packed CLI creates a working product.
- The baseline EF migrations are intentionally present in the factory payload.
