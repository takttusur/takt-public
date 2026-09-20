# AGENTS.md

Guidance for AI coding agents working in **takt-public** — the public website of the TAKT club.

## Big picture

Two independently-deployed halves, wired together for local dev only:

- **`src/takt-spa/`** — React 19 + TypeScript + Vite SPA. This is the primary, production-deployed app (nginx image + GitHub Pages). All npm commands run **from this directory**, not the repo root.
- **.NET 10 / Aspire backend** (`src/Takt.AppHost`, `src/Takt.Identity.API`, `src/Takt.ServiceDefaults`) — early-stage services. `Takt.Identity.API` handles only auth/JWT (users, login, refresh) — **never** club-member data, human PII, or business roles (see its README).
- Note: `compose.yaml` and `AppHost.cs` reference `src/Takt.Services.WebApi` which does **not exist yet** — the backend is a work in progress. Don't assume it's runnable.

**Local full-stack flow:** `docker compose up --build` → proxy on `:8080` routes `/api/*` to the .NET webapi and everything else to the Vite dev server (HMR). Or use Aspire: `dotnet run --project src/Takt.AppHost/Takt.AppHost.csproj`.

## SPA architecture & conventions

- **Feature folders** under `src/features/<name>/` are self-contained: `components/`, `data/` (RTK Query API), `store/` (slice), `types/`, `hooks/`, `utils/`. Example: `features/inmemoria/`.
- **Data fetching = RTK Query** via `createApi` in `features/*/data/*Api.ts`. Endpoints target `v1/...` under `import.meta.env.VITE_API_URL`. Use `transformResponse` to map `PagedResultModel<T>` DTOs to view models; export the generated `useXxxQuery` hooks. See `features/inmemoria/data/inmemoriaApi.ts`.
- **Store wiring:** add a feature's reducer + `api.reducerPath` to `src/reducers/rootReducer.ts`, and concat its middleware in `src/store/index.ts`. App uses `HashRouter` (see `main.tsx`) because it's served from GitHub Pages.
- **Routing** is class-based: register routes as `IAppRoute` fields on `RootAppRoutingMap` in `src/routes/index.tsx`; `getRoutes()` reflects over enumerable props.
- **Env access goes through `services/EnvironmentService`** — don't read `import.meta.env` ad-hoc for feature flags (LogRocket, Yandex.Metrika). One CSS file per component, colocated (e.g. `personPage.css`).
- **Mocking:** MSW is enabled when `VITE_MSW_ENABLED=true` (default in `.env.development`); handlers in `src/mocks/handlers.ts`, bootstrapped by `tryEnableMocking()` in `main.tsx`.
- UI text is in Russian (e.g. `'Загрузка...'`); keep it that way.

## Developer workflows (run inside `src/takt-spa/`)

- `npm install` · `npm run dev` (Vite) · `npm run build` (prod; `build:dev`, `build:github` for other modes)
- `npm run lint` — **zero-warning gate** (`--max-warnings 0`); CI fails on any warning. `npm run lint:fix`, `npm run format` (Prettier).
- `npm run test` — Vitest + Testing Library (jsdom). Test files are `*.spec.tsx` colocated with components. Node ≥ 20 required; `lint` runs as a pre-commit hook.

## Testing patterns

Component tests render with a **minimal RTK Query store** (`configureStore` with just the api reducer+middleware) wrapped in `MemoryRouter`, and mock query hooks via `vi.mock`/`vi.spyOn`. Copy the setup in `features/inmemoria/components/PersonPage.spec.tsx`.

## CI/CD

- Push to `develop` → lint + test + build → deploy to GitHub Pages + push `ghcr.io/takttusur/takt-public:develop` (`.github/workflows/develop.yml`).
- Push to `master` / tag `v*` → publish `:latest` nginx image (`.github/workflows/master-publish.yml`).
- Env vars for build (`VITE_API_URL`, Yandex.Metrika) are injected into a `.env.github` file in CI, not committed.
