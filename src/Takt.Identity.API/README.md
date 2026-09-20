# Takt.Identity.API

Headless Identity microservice for Takt Tusur. It owns authentication/authorization data only (users, global roles, invitations, refresh sessions, passkeys, auth challenges, audit events) and does not store club profile/business data.

## Architecture overview

- **ASP.NET Core Identity (`AddIdentityCore`)** with roles (`User`, `Admin`)
- **PostgreSQL + EF Core/Npgsql** (`IdentityAppDbContext`)
- **Mandatory TOTP for password authentication**
- **Passkeys/WebAuthn** via ASP.NET Core Identity passkey handler
- **JWT access tokens (RSA SHA-256)** + **refresh-token rotation** (hashed server-side)
- **JWKS endpoint** (`GET /api/v1.0/.well-known/jwks.json`) for microservice token validation
- **Invitation-only registration** (single-use, expiring, hashed tokens)
- **Admin bootstrap** as explicit one-time command

## Local requirements

- .NET SDK 10
- Docker + Docker Compose
- `dotnet-ef` tool:

```bash
dotnet tool install --global dotnet-ef
```

## Run with Docker (PostgreSQL + API)

```bash
cd src/Takt.Identity.API
docker compose -f compose.yaml up --build
```

API: `http://localhost:8081`  
Scalar API Reference: `http://localhost:8081/scalar`  
OpenAPI: `http://localhost:8081/openapi/v1.json`  
Health: `http://localhost:8081/health`

## Run locally without Docker

1. Start PostgreSQL.
2. Set `ConnectionStrings__DefaultConnection`.
3. Ensure signing key exists (`src/Takt.Identity.API/keys/jwt-private.pem` for local dev only).
4. Apply migrations:

```bash
cd src/Takt.Identity.API
dotnet ef database update
```

5. Start API:

```bash
dotnet run --project src/Takt.Identity.API/Takt.Identity.API.csproj
```

## Migrations

- Create migration:

```bash
cd src/Takt.Identity.API
dotnet ef migrations add <Name> --output-dir Persistence/Migrations
```

- Apply migration:

```bash
dotnet ef database update
```

In production, run migrations as an explicit deployment job before scaling API replicas.

## Admin bootstrap

Bootstrap is explicit and idempotent:

```bash
cd src/Takt.Identity.API
dotnet run --project Takt.Identity.API.csproj -- --bootstrap-admin
```

If admin does not exist, credentials are printed **once**:

```text
========================================
INITIAL ADMINISTRATOR CREATED

Username: admin
Password: <generated-password>
...
========================================
```

If admin already exists, no password is printed and no reset occurs.

> Security note: container stdout may be collected by log pipelines. Run bootstrap as a one-time controlled job and treat stdout as sensitive.

## Invitation flow

1. Admin creates invitation (`POST /api/v1.0/invitations`).
2. API returns token (and optional activation URL).
3. Frontend validates token (`GET /api/v1.0/invitations/{token}`).
4. User sets initial password (`POST /api/v1.0/invitations/{token}/accept`).
5. API creates account and returns `TwoFactorSetupRequired`.
6. User completes TOTP setup before receiving full tokens.

## Password + TOTP flow

### Initial login (2FA not enabled)

1. `POST /api/v1.0/auth/password/login`
2. Response:

```json
{ "status": "TwoFactorSetupRequired", "setupToken": "..." }
```

3. `POST /api/v1.0/account/2fa/setup` with `setupToken` (returns provisioning URI).
4. `POST /api/v1.0/account/2fa/enable` with `setupToken` + TOTP code.
5. API returns access/refresh tokens.

### Subsequent password login

1. `POST /api/v1.0/auth/password/login`
2. Response:

```json
{ "status": "TwoFactorVerificationRequired", "challengeToken": "..." }
```

3. `POST /api/v1.0/auth/2fa/verify` with `challengeToken` + TOTP code.
4. API returns access/refresh tokens.

No full access token is issued before required factors complete.

## Passkey enrollment and sign-in

### Enrollment (authenticated session required)

1. `POST /api/v1.0/account/passkeys/options`
2. Response includes:
   - `creationOptionsJson` (for `navigator.credentials.create(...)`)
   - `stateToken`
3. Browser creates passkey and JSON-serializes credential.
4. `POST /api/v1.0/account/passkeys/complete` with `stateToken` + `credentialJson`.

### Sign-in

1. `POST /api/v1.0/auth/passkey/options` (`userName` optional for discoverable credentials).
2. Response includes:
   - `requestOptionsJson` (for `navigator.credentials.get(...)`)
   - `stateToken`
3. Browser JSON-serializes assertion credential.
4. `POST /api/v1.0/auth/passkey/complete` with `stateToken` + `credentialJson`.
5. API verifies assertion and returns access/refresh tokens.

Passkey challenge state is single-use and expiring. User verification is required via passkey options.

## Frontend WebAuthn integration notes

- Send `navigator.credentials.create` and `navigator.credentials.get` results as JSON strings in `credentialJson`.
- Keep Base64URL values unchanged when forwarding JSON.
- Do not convert credential binary fields manually unless your frontend library requires it.

## JWT validation in other microservices

Services validate tokens offline using:

- `iss` = configured `Tokens:Issuer`
- `aud` = configured `Tokens:Audience`
- JWKS from `GET /api/v1.0/.well-known/jwks.json`

Claims include:

- `sub` (user id)
- `preferred_username`
- `role` (User/Admin)
- `auth_time`

## Key endpoints (examples)

### Create invitation (Admin)

`POST /api/v1.0/invitations`

```json
{ "userName": "andrey", "role": "User" }
```

### Accept invitation

`POST /api/v1.0/invitations/{token}/accept`

```json
{ "password": "StrongPassword!123" }
```

### Password login

`POST /api/v1.0/auth/password/login`

```json
{ "userName": "andrey", "password": "StrongPassword!123" }
```

### Refresh

`POST /api/v1.0/auth/refresh`

```json
{ "refreshToken": "<token>" }
```

## Configuration (important)

- `ConnectionStrings__DefaultConnection`
- `Tokens__Issuer`
- `Tokens__Audience`
- `Tokens__PrivateKeyPemPath` (**Development only**) or `Tokens__PrivateKeyPem` (recommended for non-development secrets)
- `Passkeys__ServerDomain`
- `Passkeys__AllowedOrigins__*`
- `Cors__AllowedOrigins__*`
- `Network__HttpsRedirectionEnabled` (set `false` behind reverse proxy if HTTP access should remain HTTP)
- `Network__ExternalBaseUrl` (optional explicit public base URL, e.g. `http://localhost:3333/identity`, used by OpenAPI/Scalar servers)
- `Bootstrap__AdminUserName`

Use HTTPS and correct reverse-proxy forwarded headers in production, and set passkey RP/origins to real domains.

## Tests

```bash
dotnet test
```

## Production security considerations

- Keep RSA private key in secrets manager or mounted secret volume (never regenerate per restart).
- Persist and protect PostgreSQL volume.
- Restrict CORS to trusted frontend origins.
- Run bootstrap as controlled one-time operation.
- Enforce HTTPS/TLS termination and forwarded-header configuration behind reverse proxies/k3s ingress.
