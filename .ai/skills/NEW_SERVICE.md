# C# Microservice Engineering Skill

Use this skill when creating, modifying, reviewing, or extending a C#/.NET microservice.

The goal is to produce maintainable production code with clear boundaries between transport, application logic, domain logic, and infrastructure.

Optimize for:

1. Business capability ownership.
2. Fast and safe changes.
3. Low coupling.
4. Independent deployment.
5. Testability.
6. Operational reliability.
7. Simplicity proportional to the problem.

Do not introduce architectural patterns merely because they are fashionable.

A simple CRUD service should remain simple.

A service with complex business rules should explicitly model those rules.

---

# 1. Core Architecture Rules

A microservice owns one cohesive business capability.

It owns:

- its business logic;
- its persistence;
- its public API;
- its emitted integration events;
- its integration with external systems.

It must not rely on:

- another service's database;
- shared database tables;
- file-based integration;
- implementation details of another service.

Allowed service-to-service integration mechanisms are normally:

- HTTP/REST for synchronous interactions;
- messaging/event bus for asynchronous interactions.

The service should remain operational as far as reasonably possible when another service is unavailable.

---

# 2. Dependency Direction

Dependencies must point inward.

Conceptually:

```text
API / Transport
      |
      v
Application
      |
      v
Domain

Infrastructure
      |
      +---- implements interfaces required by Application/Domain
```

Infrastructure may depend on Application or Domain abstractions.

Domain code must never depend on:

- ASP.NET Core;
- Entity Framework Core;
- message brokers;
- HTTP clients;
- configuration providers;
- logging frameworks;
- cloud SDKs.

Avoid cyclic project or namespace dependencies.

---

# 3. API Layer

The API layer is a transport adapter.

Its responsibilities are limited to:

- HTTP routing;
- authentication/authorization;
- request deserialization;
- protocol-level validation;
- invoking an application use case;
- mapping the result to HTTP;
- returning appropriate status codes and Problem Details.

Controllers/endpoints must remain thin.

## Forbidden

Never write code like:

```csharp
public class TripsController(ITripRepository repository)
{
    public async Task<IActionResult> Create(CreateTripRequest request)
    {
        var trip = new Trip(...);

        await repository.AddAsync(trip);

        return Ok(trip);
    }
}
```

Controllers and endpoints MUST NOT directly use:

- repositories;
- DbContext;
- database connections;
- Dapper;
- message brokers;
- external service SDKs;
- business workflows.

Instead:

```text
HTTP request
    ↓
Controller / Endpoint
    ↓
Application use case
    ↓
Domain + ports
    ↓
Infrastructure adapters
```

---

# 4. API Contracts

Public API contracts must be explicitly modeled.

Request and response contracts must not be hidden inside controller files.

Bad:

```text
TripsController.cs
    TripsController
    CreateTripRequest
    CreateTripResponse
    TripDto
    UpdateTripRequest
```

Preferred:

```text
Features/
  Trips/
    CreateTrip/
      CreateTripRequest.cs
      CreateTripResponse.cs
      CreateTripEndpoint.cs
```

or:

```text
Contracts/
  Trips/
    CreateTripRequest.cs
    CreateTripResponse.cs
```

Use one meaningful public type per file unless several tiny private/internal implementation types are inseparable.

Do not place unrelated records/classes in the same source file merely because C# allows it.

Public contracts are NOT domain entities.

Do not expose EF entities or domain aggregates directly through HTTP.

Explicitly map:

```text
API Contract <-> Application/Domain
```

Prefer immutable request/response DTOs where appropriate.

Example:

```csharp
public sealed record CreateTripRequest(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate);
```

---

# 5. Application Layer

Every externally triggered operation should correspond to an explicit application use case.

Examples:

```text
CreateTrip
ApproveApplication
RegisterParticipant
CancelBooking
GetTripDetails
SearchMembers
```

An application use case coordinates work.

It may:

- load domain objects;
- invoke domain behavior;
- call abstractions for persistence;
- call external-service abstractions;
- publish or enqueue events;
- manage transaction boundaries.

It should NOT contain protocol-specific HTTP logic.

Example:

```csharp
public sealed class CreateTripHandler
{
    public async Task<CreateTripResult> Handle(
        CreateTripCommand command,
        CancellationToken cancellationToken)
    {
        ...
    }
}
```

The exact mechanism is optional.

Acceptable approaches include:

- plain application services;
- command/query handlers;
- vertical slices;
- MediatR;
- another lightweight mediator.

Do NOT introduce MediatR simply to call one class from another.

The important requirement is the architectural boundary, not the library.

---

# 6. Vertical Slice Preference

Prefer organizing application behavior around business features/use cases rather than technical categories.

Good:

```text
Features/
  Trips/
    Create/
    Cancel/
    GetDetails/
  Members/
    Register/
    Suspend/
```

Be cautious with structures dominated by generic folders such as:

```text
Controllers/
Services/
Repositories/
Models/
Helpers/
Utils/
```

These frequently become unrelated dumping grounds.

Technical layers may still exist as projects or boundaries while implementation inside them is organized by feature.

---

# 7. Domain Model

Use the simplest domain model that correctly represents the business.

For simple CRUD:

- simple entities/data models are acceptable;
- do not invent aggregates, factories, specifications, or domain events unnecessarily.

For non-trivial business rules:

- model behavior inside domain objects;
- protect invariants;
- use value objects where they provide real value;
- use aggregate boundaries for transactional consistency.

Bad:

```csharp
trip.Status = TripStatus.Approved;
```

when approval has rules.

Prefer:

```csharp
trip.Approve(actor, clock.UtcNow);
```

when approval represents real domain behavior.

Domain objects must not rely on callers remembering to maintain invariants.

---

# 8. Persistence

Persistence implementation belongs to Infrastructure.

Example:

```text
Infrastructure/
  Persistence/
    AppDbContext.cs
    Configurations/
    Repositories/
    Migrations/
```

Do not inject `DbContext` into controllers.

Do not access persistence directly from HTTP endpoints.

For complex domain writes, prefer repositories aligned with aggregate roots.

Example:

```csharp
public interface ITripRepository
{
    Task<Trip?> GetAsync(
        TripId id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Trip trip,
        CancellationToken cancellationToken);
}
```

Avoid repositories that merely reproduce the complete EF Core API:

```csharp
IRepository<T>
{
    IQueryable<T> Query();
    Add(...);
    Update(...);
    Delete(...);
    ...
}
```

Such abstractions often add complexity without providing a meaningful boundary.

For read-only queries, pragmatic approaches are allowed:

- EF Core projections;
- Dapper;
- SQL;
- dedicated query services.

Commands and queries do not have to use identical persistence mechanisms.

Never create cross-service database joins.

---

# 9. Transactions

A local database transaction must remain inside one microservice.

Never create distributed database transactions between services.

If a business operation spans services, prefer:

- asynchronous events;
- eventual consistency;
- saga/process manager when coordination is genuinely required.

Keep transaction boundaries explicit.

Avoid multiple `SaveChangesAsync()` calls during one atomic use case unless intentionally required.

---

# 10. Messaging

Integration events represent facts that already happened.

Prefer names such as:

```text
TripCreated
ParticipantRegistered
ApplicationApproved
PaymentReceived
```

Avoid command-like event names such as:

```text
CreateTrip
RegisterParticipant
ProcessPayment
```

unless the message actually represents a command.

Event contracts are public integration contracts.

They must be explicitly defined and documented.

Consumers must follow the tolerant reader principle:

- ignore unknown fields;
- avoid assuming optional fields are always present;
- prefer additive evolution;
- do not break existing consumers unnecessarily.

Do not put business logic inside the message broker or integration infrastructure.

The endpoint is smart; the transport is dumb.

---

# 11. Reliable Event Publishing

Never assume this sequence is reliable:

```text
UPDATE database
publish event
```

because the service can crash between these operations.

For business-critical integration events, prefer the Transactional Outbox pattern:

```text
Database transaction:
    update business state
    insert outbox message

Commit

Background publisher:
    read outbox
    publish
    mark processed
```

Consumers must assume at-least-once delivery unless the infrastructure explicitly guarantees otherwise.

Therefore consumers should be idempotent.

Use Inbox/deduplication where duplicate processing could cause incorrect state.

---

# 12. Idempotency

Operations likely to be retried must be designed for idempotency.

Examples:

- payment callbacks;
- event consumers;
- job execution;
- external webhook processing;
- commands submitted by unreliable clients.

Possible mechanisms include:

- idempotency keys;
- unique database constraints;
- processed-message tables;
- optimistic concurrency;
- natural business identifiers.

Never rely solely on:

```csharp
if (!Exists(...))
    Insert(...);
```

when concurrent processing is possible.

Enforce uniqueness at the persistence layer where appropriate.

---

# 13. External HTTP Calls

Never instantiate arbitrary `HttpClient` instances in business code.

Use typed or named clients through `IHttpClientFactory`.

Every network call must have bounded execution time.

Apply where appropriate:

- timeout;
- retry;
- circuit breaker;
- concurrency limiting;
- rate limiting.

Do not blindly retry every request.

Automatic retries are normally appropriate only when the operation is safe/idempotent or the remote API explicitly supports idempotency.

Propagate `CancellationToken`.

---

# 14. Failure Handling

Design for partial failure.

Assume:

- databases become temporarily unavailable;
- DNS can fail;
- external services time out;
- event delivery can happen more than once;
- pods can terminate during execution;
- requests can be cancelled;
- network connections can disappear.

Do not catch exceptions merely to log and rethrow them.

Bad:

```csharp
try
{
    ...
}
catch (Exception ex)
{
    logger.LogError(ex, "Error");
    throw;
}
```

unless this boundary adds meaningful context.

Handle errors at the appropriate boundary.

Use centralized exception handling for HTTP APIs.

Return RFC-compatible Problem Details where applicable.

Do not expose internal stack traces or database errors to API consumers.

---

# 15. Validation

There are different kinds of validation.

## Transport validation

Examples:

- required JSON field;
- malformed UUID;
- invalid enum representation;
- impossible string length.

Perform this at the application/API boundary.

## Business validation

Examples:

- registration closes after a deadline;
- trip cannot be cancelled after completion;
- participant cannot join without required approval.

These rules belong in the application/domain model.

Do not rely solely on request validators to protect domain invariants.

---

# 16. Concurrency

Assume requests can run concurrently.

Do not implement critical consistency rules only in memory.

Use appropriate mechanisms such as:

- optimistic concurrency tokens;
- unique constraints;
- atomic SQL operations;
- row locking when justified;
- compare-and-set semantics.

Avoid broad database locks.

Keep locked transactions short.

Never use application-wide `lock` as a substitute for distributed consistency.

---

# 17. Cancellation and Async Code

All I/O operations should be asynchronous.

Propagate `CancellationToken` through:

```text
HTTP endpoint
    ↓
Application use case
    ↓
Repository / HTTP client / messaging
```

Avoid:

```csharp
.Result
.Wait()
Task.Run(...)
```

around naturally asynchronous I/O.

Do not create fake async methods that merely wrap synchronous work.

---

# 18. Time

Business code must not depend directly on the system clock when time affects behavior.

Avoid:

```csharp
DateTime.UtcNow
```

deep inside domain/application logic.

Prefer an injectable abstraction such as:

```csharp
TimeProvider
```

This makes time-dependent behavior deterministic and testable.

Use UTC internally unless the domain explicitly requires local civil time.

Use `DateOnly` or `TimeOnly` when the business concept is actually a date or local time rather than an instant.

---

# 19. Identifiers

Use strongly meaningful identifiers when doing so improves correctness.

Avoid passing unrelated `Guid` values everywhere when confusing identifiers could cause bugs.

For example:

```csharp
public readonly record struct TripId(Guid Value);
public readonly record struct MemberId(Guid Value);
```

Do not introduce strongly typed IDs mechanically for trivial services.

---

# 20. Configuration and Secrets

Configuration must come from configuration providers/environment.

Never hardcode:

- passwords;
- API keys;
- tokens;
- connection strings;
- private endpoints.

Use options objects for non-trivial configuration.

Validate required configuration at application startup when possible.

Do not use configuration as a hidden service locator.

---

# 21. Observability

Every production service must provide useful observability.

Prefer OpenTelemetry-compatible:

- logs;
- metrics;
- distributed traces.

Include correlation/trace context across service boundaries.

Log structured values:

```csharp
logger.LogInformation(
    "Trip {TripId} approved by member {MemberId}",
    trip.Id,
    memberId);
```

Avoid string interpolation:

```csharp
logger.LogInformation(
    $"Trip {trip.Id} approved by {memberId}");
```

Do not log secrets, access tokens, passwords, or sensitive personal data.

Important business operations should be observable without attaching a debugger.

---

# 22. Health Checks

Every HTTP microservice should expose:

- liveness;
- readiness.

Liveness answers:

> Is this process alive enough that restarting it might help?

Readiness answers:

> Can this instance currently accept traffic?

Do not make liveness dependent on every downstream service.

Otherwise a downstream outage can cause unnecessary restart loops.

Readiness may verify dependencies required to serve requests.

---

# 23. API Compatibility

Public APIs are contracts.

Do not casually rename or remove:

- fields;
- routes;
- event properties;
- semantic meanings.

Prefer additive changes.

Unless the existing system explicitly requires versioned APIs, do not introduce `/v1`, `/v2`, etc. automatically.

Prefer evolving contracts compatibly.

Breaking changes require an explicit migration strategy.

---

# 24. Security

Use deny-by-default thinking.

Authorization belongs at meaningful business boundaries.

Authentication alone is not authorization.

Do not trust:

- client-provided user IDs;
- roles supplied in request bodies;
- ownership fields supplied by clients.

Derive identity from authenticated context.

Use parameterized database access.

Never concatenate user input into SQL.

Apply least privilege to:

- database accounts;
- cloud identities;
- service credentials.

---

# 25. Testing

Tests should follow architectural boundaries.

At minimum consider:

## Domain unit tests

For business rules and invariants.

These should require no database or network.

## Application tests

For use-case orchestration.

Use test doubles only at meaningful external boundaries.

## Infrastructure integration tests

Use the real persistence technology where feasible.

For EF Core behavior, prefer testing against the actual database engine using containers rather than relying on EF InMemory as proof that production SQL works.

## API tests

Test important external contracts end-to-end through HTTP.

Prioritize behavior over implementation details.

Do not write tests merely to increase coverage percentages.

---

# 26. Project Structure

Do not blindly create many projects.

For a small service, this may be enough:

```text
Takt.Trips/
  Features/
  Domain/
  Infrastructure/
  Program.cs

Takt.Trips.Tests/
```

For a larger service:

```text
Takt.Trips.Api
Takt.Trips.Application
Takt.Trips.Domain
Takt.Trips.Infrastructure

Takt.Trips.UnitTests
Takt.Trips.IntegrationTests
```

Choose complexity according to the business problem.

Physical project separation is useful when it enforces meaningful dependency boundaries.

Do not create projects that contain only two trivial forwarding classes.

---

# 27. File Organization

Prefer small cohesive files.

A file should normally contain one main concept.

Good:

```text
CreateTripRequest.cs
CreateTripResponse.cs
CreateTripCommand.cs
CreateTripHandler.cs
Trip.cs
TripRepository.cs
TripConfiguration.cs
```

Avoid enormous files containing:

```text
controller
requests
responses
domain models
mapping
database logic
helper classes
```

Do not create generic dumping-ground files such as:

```text
Models.cs
Dtos.cs
Helpers.cs
Common.cs
Utils.cs
```

when meaningful names are available.

---

# 28. Dependency Injection

Use constructor injection.

Avoid service locator patterns:

```csharp
serviceProvider.GetRequiredService<T>()
```

inside business code.

Avoid classes with excessive dependencies.

If a use case requires 8-10 unrelated dependencies, reconsider its responsibilities.

Register dependencies near the composition root.

Infrastructure registration should normally be encapsulated behind extension methods such as:

```csharp
services.AddPersistence(configuration);
services.AddMessaging(configuration);
```

but do not hide business behavior inside DI registration.

---

# 29. Mapping

Mapping belongs at boundaries.

Do not allow external DTOs to spread throughout domain code.

Avoid adding AutoMapper solely to map three properties.

Explicit mapping is often clearer:

```csharp
return new TripResponse(
    trip.Id.Value,
    trip.Name,
    trip.StartDate,
    trip.EndDate);
```

Use mapping libraries only when they materially reduce repetitive code without hiding important behavior.

---

# 30. Generated Code Quality Rules

When generating code:

1. First inspect the existing project structure and conventions.
2. Reuse existing abstractions when they are sensible.
3. Do not introduce a new architectural pattern for a single feature without justification.
4. Keep changes local to the requested business capability.
5. Do not create speculative abstractions for hypothetical future requirements.
6. Prefer explicit code over reflection or magic.
7. Prefer standard .NET functionality over adding dependencies.
8. Do not add NuGet packages unless they solve a concrete problem.
9. Never suppress compiler warnings without understanding their cause.
10. Never leave empty catch blocks.
11. Never swallow cancellation.
12. Never use `async void` except legitimate event handlers.
13. Propagate cancellation tokens.
14. Dispose owned disposable resources correctly.
15. Enable nullable reference types.
16. Prefer immutable state where practical.
17. Avoid mutable global/static state.
18. Avoid hidden I/O inside properties or constructors.

---

# 31. Architecture Smells

Treat the following as warning signs.

## Controller -> Repository

```text
Controller -> Repository -> Database
```

Usually missing an application use case.

## Controller -> DbContext

Strong violation of transport/persistence separation for non-trivial services.

## Giant Service Class

```text
TripService.cs
  Create
  Update
  Delete
  Search
  Register
  Cancel
  Approve
  Export
  Notify
  ...
```

Split by use case/business behavior.

## Generic Repository Everywhere

May hide EF capabilities without adding a useful abstraction.

## Domain Depending on EF Core

Violates infrastructure independence.

## Shared Database

Creates runtime and release coupling between services.

## Shared "Common" Library Containing Business Models

Can silently turn independent microservices into a distributed monolith.

Share technical primitives sparingly.

Do not share domain models between services.

## Events Used as Remote Procedure Calls

If service A emits `PleaseCreateSomething` and synchronously waits for service B's result, reconsider whether messaging is the right integration mechanism.

---

# 32. AI Generation Procedure

Before writing code, silently determine:

1. What business capability owns this feature?
2. What is the use case?
3. What is the external contract?
4. What business rules exist?
5. What data does this service own?
6. Is the operation transactional?
7. Can it be retried?
8. Does it produce an integration event?
9. What happens when dependencies fail?
10. What needs to be observable?
11. What concurrency scenarios can occur?
12. What tests demonstrate the behavior?

Then generate the minimum architecture required to solve the problem correctly.

Do not expose this internal checklist unless explicitly asked.

---

# 33. Mandatory Pre-Completion Review

Before completing a code generation task, verify:

- [ ] Controller/endpoint does not access repository or DbContext directly.
- [ ] API request/response types are not dumped into controller files.
- [ ] Public contracts are separate from domain entities.
- [ ] Business logic is not implemented in controllers.
- [ ] Infrastructure concerns are outside Domain.
- [ ] Service does not access another service's database.
- [ ] CancellationToken is propagated through I/O.
- [ ] External calls have reasonable timeout/resilience behavior.
- [ ] Retried operations are idempotent where necessary.
- [ ] Critical event publishing cannot be lost between DB commit and publish.
- [ ] Concurrent execution has been considered.
- [ ] Errors are converted into appropriate API responses.
- [ ] Structured logs/traces exist for important operations.
- [ ] Health endpoints exist for deployable services.
- [ ] No secrets are committed to source code.
- [ ] Tests cover important business behavior.
- [ ] No unnecessary abstraction/package/project was introduced.

If any item fails, fix the design before presenting the final implementation.

---

# 34. Preferred Default

Unless the existing repository dictates otherwise, prefer:

```text
ASP.NET Core API
        ↓
thin endpoint/controller
        ↓
feature/use-case handler
        ↓
domain model
        ↓
ports/interfaces
        ↓
infrastructure adapters
```

Organize implementation primarily around business capabilities and vertical slices.

Use richer DDD patterns only where domain complexity justifies them.

The objective is not maximum architectural purity.

The objective is software that can be understood, changed, tested, deployed, and operated independently.