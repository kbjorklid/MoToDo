# Architectural description

This document provides a comprehensive guide to structuring a .NET application as a **Modular Monolith**. This architectural style is designed to produce a single, deployable application that is internally composed of well-encapsulated, independent modules. It serves as a practical blueprint for implementing several key software design principles:

* **Domain-Driven Design (DDD):** Aligning the software structure with the business domain by breaking it down into logical modules and using concepts like Aggregates, Entities, and Value Objects.
* **Clean Architecture / Hexagonal Architecture:** Enforcing a clear separation of concerns and a strict, inward-facing dependency flow within each module. External services are accessed through ports and adapters.
* **CQRS (Command Query Responsibility Segregation):** Separating the logic for changing state (Commands) from the logic for reading state (Queries) for improved performance and clarity.
* **Modular Monolith:** Achieving the maintainability and team autonomy benefits often associated with microservices, but without the operational complexity of a distributed system.

## Core Architectural Concepts

Our architecture is built upon a few foundational concepts that work together to create a robust and scalable system.

### The Modular Monolith

This architecture organizes the application as a **Modular Monolith**. The entire system is built and deployed as a single unit, but its internal structure is composed of loosely coupled, highly cohesive modules. This approach provides the maintainability and clear boundaries of a microservices architecture while avoiding the operational complexity of a distributed system. The primary goal is to enforce logical separation and independent development of different business capabilities within a single process.

### Modules as Bounded Contexts

Each **Module** in this solution is a direct, physical implementation of a **Bounded Context** from our Domain-Driven Design (DDD) model. It represents a conceptual boundary around a specific business capability, complete with its own ubiquitous language. In practice, a module encapsulates its own data and logic, acting as the authority for a distinct area of the business domain. Communication between these modules is explicit and follows strict rules to maintain their independence.

### Clean Architecture Within Each Module

Every module in the monolith internally adheres to the principles of Clean Architecture, which is a specific implementation of the Ports and Adapters (or Hexagonal) Architecture. The most critical principle is the **Dependency Rule**: source code dependencies can only point inwards, toward the core business logic.

`Domain <- Application <- Infrastructure`

* **Domain:** The heart of the module. It contains the core business logic, entities, and aggregates. It has **zero dependencies** on any other project.
* **Application:** The orchestrator. It contains use cases (as command/query handlers) that drive the domain logic. It depends only on the Domain layer and the module's own Contracts layer.
* **Infrastructure:** The implementation details. It contains databases and clients for external services. It implements interfaces defined in the Application and Domain layers.

Domain and Application may define port interfaces. The Infrastructure layer then defines the implementations for these ports.

For example, Application layer may define `IEmailSenderPort`, and the infrastructure layer might have a concrete class implementing the port interface, perhaps `SendGridAdapter`.

### CQRS (Command Query Responsibility Segregation)

We strictly separate the models used for writing data from those used for reading data.
*   **Commands** (Writes) are operations that change state. They use the full Domain model (Aggregates, Entities) to enforce all business rules and invariants.
*   **Queries** (Reads) are operations that retrieve data. For performance, they bypass the Domain model and repositories, querying a data source directly and projecting the results into Data Transfer Objects (DTOs).

## Project File Structure

The physical layout of the code is a direct reflection of these architectural principles.

```text
/YourApp.sln
|
└─── src
     |
     ├─── BuildingBlocks
     │    ├─── Base.Domain/
     │    │    └── Base.Domain.csproj
     │    ├─── Base.Application/
     │    │    └── Base.Application.csproj
     │    └─── Base.Infrastructure/
     │         └── Base.Infrastructure.csproj
     |
     ├─── ModuleA
     │    ├── ModuleA.Contracts/
     │    │   └── ModuleA.Contracts.csproj
     │    ├── ModuleA.Domain/
     │    │   └── ModuleA.Domain.csproj
     │    ├── ModuleA.Application/
     │    │   └── ModuleA.Application.csproj
     │    └── ModuleA.Infrastructure/
     │        └── ModuleA.Infrastructure.csproj
     |
     ├─── ModuleB
     │    ├── ModuleB.Contracts/
     │    │   └── ModuleB.Contracts.csproj
     │    ├── ModuleB.Domain/
     │    │   └── ModuleB.Domain.csproj
     │    ├── ModuleB.Application/
     │    │   └── ModuleB.Application.csproj
     │    └── ModuleB.Infrastructure/
     │        └── ModuleB.Infrastructure.csproj
     |
     └─── ApiHost
          └── ApiHost.csproj                // ASP.NET Core Web API Project
```

### The `BuildingBlocks` Module

This special module does not represent a business domain. Instead, it contains shared, cross-cutting concerns and base types that are reusable across multiple modules.

*   **`Base.Domain`**: Includes base classes for domain-driven design constructs (e.g., `Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`). This ensures consistency and reduces boilerplate in the domain layers of other modules.
*   **`Base.Application`**: Provides reusable components for the application layer, such as generic behaviors for command/query pipelines (e.g., logging, validation).
*   **`Base.Infrastructure`**: Contains common infrastructure logic, such as a generic repository implementation or shared services for messaging or caching.

### Project Dependencies

The project dependencies flow inwards, creating a directed acyclic graph.

1.  **`ApiHost` (Presentation)**
    *   References: `ModuleX.Contracts` (for all modules it exposes).
2.  **`ModuleX.Infrastructure`**
    *   References: `ModuleX.Application`, `Base.Infrastructure`.
3.  **`ModuleX.Application` (Orchestration)**
    *   References: `ModuleX.Domain`, `ModuleX.Contracts`, `Base.Application`.
    *   May reference `ModuleY.Contracts` to communicate with another module.
4.  **`ModuleX.Domain` (Core Business Logic)**
    *   References: `Base.Domain`.
5.  **`ModuleX.Contracts`**
    *   References: None.
6.  **`BuildingBlocks` Projects**
    *   `Base.Application` references `Base.Domain`.
    *   `Base.Infrastructure` references `Base.Application`.

## Layer Implementation Guidance

### 1. `ModuleX.Contracts`
* **Contents:**
    * The public API of the module.
    * Definitions for `Commands`, `Queries`, and `Events` that the module publishes or handles.
    * Public DTOs used in the contracts.
* **Rules:**
    * This project should have no dependencies on any other project.
    * It defines the boundary for inter-module communication.

### 2. `ModuleX.Domain`

* **Contents:**
    * Aggregate Roots, Entities, Value Objects.
    * Domain Events (internal to the module).
    * Domain Services.
    * Repository *interfaces* (e.g., `IOrderRepository`).
* **Rules:**
    * All business logic must be inside Aggregate Root methods or in Domain Services.
    * This project must have NO external dependencies.

### 3. `ModuleX.Application`

* **Contents:**
    * Command and Query Handlers for messages defined in `ModuleX.Contracts`.
    * Port interfaces to external systems.
    * `ServiceCollectionExtensions.cs` with `.AddModuleXApplicationServices()` method.
* **Rules:**
    * **Command Handlers (Writes):**
        1.  Load the full Aggregate Root using a repository interface.
        2.  Execute the business operation by calling a method on the Aggregate.
        3.  Persist the Aggregate using the repository interface.
    * **Query Handlers (Reads):**
        1.  Bypass repositories and the domain model.
        2.  Query the data source (`DbContext` or Dapper) directly.
        3.  Project the results into a DTO (often defined in `ModuleX.Contracts`) and return it.

### 4. `ModuleX.Infrastructure`

* **Contents:**
    * EF Core `DbContext`.
    * Concrete implementations of repository interfaces.
    * Database migrations.
    * Adapters for external services.
    * `ServiceCollectionExtensions.cs` with `.AddModuleXInfrastructureServices()` method.
* **Rules:**
    * Implement the interfaces defined in the Domain and Application layers. This layer handles all I/O.
    * Infrastructure implementations should be `internal` when possible, exposed only through DI extension methods.

### 5. `ApiHost` (Presentation Layer)

* **Contents:**
    * ASP.NET Core REST controllers.
    * API-specific DTOs for request/response models.
* **Rules:**
    * Controllers must be "thin."
    * **API Isolation Requirement:** Controllers must NOT use module contract objects directly in the REST API. Instead, define separate API DTOs to prevent tight coupling between external API surface and internal module contracts.
    * An API endpoint's only responsibilities are:
        1.  Receive an HTTP request using API-specific DTOs.
        2.  Map API DTOs to module contracts (Commands/Queries).
        3.  Send the corresponding Command or Query to the message bus.
        4.  Map internal results back to API response DTOs.
        5.  Return the result as an HTTP response.

## Inter-Module Communication in Action

The new golden rule is: **Modules must only reference the `Contracts` project of other modules they need to communicate with.** 
Direct references to `Application`, `Domain`, or `Infrastructure` projects of other modules are strictly forbidden. Communication is still mediated by a message bus, but the message contracts are now owned by the modules themselves.

### Cross-Module ID Types Pattern

When a module needs to reference entities from another module (e.g., `UserId` from the Users module), it should **redefine the ID type locally** rather than importing it from the foreign module's Contracts. This maintains module autonomy and prevents tight coupling.

**Pattern:**
- Each module defines its own version of foreign ID types
- For foreign module IDs, use a simple wrapper implementation: `public sealed record struct UserId(Guid Value)`
- This ensures modules remain independent while still maintaining type safety

**Example:**
```csharp
// In ToDoLists.Domain - do NOT import Users.Contracts.UserId
// Instead, define locally:
public sealed record struct UserId(Guid Value);
```

This pattern preserves module boundaries while allowing type-safe references to foreign entities.

Let's trace a generic cross-module workflow:

1.  **Request:** A `POST` request hits an endpoint on a controller in the `ApiHost` project.
2.  **Initial Command:** The controller builds a command defined in `ModuleA.Contracts` and dispatches it to the message bus.
    ```csharp
    // In ApiHost, which references ModuleA.Contracts
    using ModuleA.Contracts;

    [HttpPost]
    public async Task<IActionResult> DoSomething([FromBody] SomeRequest request) {
        var command = new DoSomethingInModuleACommand(request.Data);
        await _commandBus.SendAsync(command);
        return Accepted();
    }
    ```
3.  **First Handler (Module A):** A handler in `ModuleA.Application` receives this command. To complete its task, it needs data from `ModuleB`. `ModuleA.Application` adds a project reference to `ModuleB.Contracts`.
    ```csharp
    // In ModuleA.Application, which references ModuleB.Contracts
    using ModuleB.Contracts;

    public async Task Handle(DoSomethingInModuleACommand command) {
        // ... logic specific to Module A ...

        // Dispatch a query defined in ModuleB's public contract
        var requiredData = await _queryBus.AskAsync(new GetDataFromModuleBQuery(...));

        // ... use requiredData to complete the logic ...
        await _repository.SaveChangesAsync();
    }
    ```
4.  **Second Handler (Module B):** A handler in `ModuleB.Application` is registered to handle `GetDataFromModuleBQuery` (which is defined in its own `ModuleB.Contracts` project). It fetches the data and returns the result.
    ```csharp
    // In ModuleB.Application
    using ModuleB.Contracts;

    public async Task<ResultDto> Handle(GetDataFromModuleBQuery query) {
        var entity = await _repository.GetByIdAsync(query.EntityId);
        // ResultDto is likely defined in ModuleB.Contracts
        return new ResultDto { Value = entity.SomeValue };
    }
    ```
5.  **Completion:** Control and the `ResultDto` return to the `DoSomethingInModuleACommandHandler`, which can now complete its original task.

This sequence allows modules to interact in a controlled way, exposing only their public API (`Contracts`) while hiding their implementation details.

## API Design Principles

### Separation of API and Module Contracts

The external REST API must remain stable and independent from internal module evolution. To achieve this:

**Problem:** Using module contract objects directly in REST controllers creates tight coupling between the external API surface and internal module structure. Changes to contract property names or structure immediately break external clients and violate backward compatibility.

**Solution:** Implement separate API DTOs that act as a translation layer between the external API and internal contracts.

#### Implementation Pattern

```csharp
// API-specific DTOs (in controller file or dedicated namespace)
public sealed record CreateToDoListApiRequest(string UserId, string Title);
public sealed record CreateToDoListApiResponse(string Id, string UserId, string Title, DateTime CreatedAt);

[HttpPost]
public async Task<IActionResult> CreateToDoList([FromBody] CreateToDoListApiRequest request)
{
    CreateToDoListCommand command = ToCommand(request);
    Result<CreateToDoListResult> result = await _messageBus.InvokeAsync<Result<CreateToDoListResult>>(command);
    
    if (result.IsSuccess)
    {
        CreateToDoListApiResponse response = ToApiResponse(result.Value);
        return CreatedAtAction(nameof(GetToDoList), new { id = response.Id }, response);
    }
    
    return HandleError(result.Error);
}

private IActionResult HandleError(Error error)
{
    // Transform internal Error objects to REST API error format
    var errorResponse = new
    {
        errors = new[]
        {
            new
            {
                code = error.Code,
                message = error.Description,
                field = ExtractFieldFromError(error) // Optional: extract field info if available
            }
        }
    };

    return error.Type switch
    {
        ErrorType.Validation => BadRequest(errorResponse),
        ErrorType.NotFound => NotFound(errorResponse),
        ErrorType.Conflict => Conflict(errorResponse),
        _ => StatusCode(500, errorResponse)
    };
}

private static string? ExtractFieldFromError(Error error)
{
    // Extract field information from error code or description if following conventions
    // e.g., "Email.InvalidFormat" → "email", "UserId.Empty" → "userId"
    if (error.Code.Contains('.'))
    {
        var parts = error.Code.Split('.');
        return parts[0].ToLowerFirst(); // Convert "Email" → "email", "UserId" → "userId"
    }
    return null;
}

private static string ToLowerFirst(this string str) =>
    string.IsNullOrEmpty(str) ? str : char.ToLowerInvariant(str[0]) + str[1..];
}

private static CreateToDoListCommand ToCommand(CreateToDoListApiRequest request)
{
    return new CreateToDoListCommand(request.UserId, request.Title);
}

private static CreateToDoListApiResponse ToApiResponse(CreateToDoListResult createResult) 
{
    return new CreateToDoListApiResponse( 
        createResult.ToDoListId.ToString(),
        createResult.UserId.ToString(), 
        createResult.Title,
        createResult.CreatedAt);
}
```

#### Implementation Guidelines

- **Separate Mapping Methods:** Extract mapping logic into dedicated static methods (`ToCommand`, `ToApiResponse`) **only when transformation is required**. Avoid creating methods that simply pass parameters through without any data transformation.
- **Direct Construction:** When parameters map directly without transformation, construct objects inline rather than creating unnecessary wrapper methods
- **Consistent Naming:** Use consistent naming patterns for mapping methods across controllers
- **Type Safety:** Maintain strong typing throughout the mapping process
- **Error Handling:** Map internal error types to appropriate HTTP status codes and problem details

**When to use mapping methods:**
```csharp
// ✅ Good - Transforms data (Guid to string, property mapping)
private static ToDoListSummaryApiDto ToApiDto(ToDoListSummaryDto dto)
{
    return new ToDoListSummaryApiDto(
        dto.Id.ToString(),        // Transformation: Guid → string
        dto.Title,
        dto.TodoCount,
        dto.CreatedAt,
        dto.UpdatedAt);
}
```

**When to avoid mapping methods:**
```csharp
// ❌ Redundant - No transformation, just parameter passing
private static GetToDoListsQuery ToQuery(string userId, string? sort, int? page, int? limit)
{
    return new GetToDoListsQuery(userId, page, limit, sort);
}

// ✅ Better - Direct construction
var query = new GetToDoListsQuery(userId, page, limit, sort);
```

#### API DTO Mapping Decision Tree

Use this decision framework to determine when to create mapping methods:

```
Does the mapping involve data transformation?
├─ YES → Create mapping method
│   ├─ Type conversions (Guid → string, DateTime → ISO string)
│   ├─ Property name changes (internal → API naming)
│   ├─ Structure changes (flattening, nesting)
│   ├─ Format changes (enum → string, complex → simple)
│   └─ Data enrichment/filtering
│
└─ NO → Use direct construction
    ├─ Parameters map 1:1 without changes
    ├─ Same types, same property names
    ├─ No business logic in mapping
    └─ Simple parameter passing

Examples of transformations requiring mapping methods:
✅ UserId.Value (Guid) → string
✅ UserStatus.Active (enum) → "active" (string)  
✅ Internal property names → API camelCase
✅ Multiple internal objects → single API response
✅ Complex validation result → simple error response

Examples NOT requiring mapping methods:
❌ string → string (same value)
❌ int → int (same value)
❌ Simple parameter reordering
❌ Constructor calls with identical parameters
```

#### Benefits

- **API Stability:** External API remains stable when internal contracts evolve
- **Independent Evolution:** API structure can be optimized for client needs without affecting internal design
- **Backward Compatibility:** Breaking changes to internal contracts don't affect external clients
- **Versioning Support:** Enables independent API versioning strategies
- **Clear Separation:** Clean boundary between external interface and internal implementation
- **Validation Flexibility:** Allows different validation rules for API vs internal contracts
- **Maintainable Mapping:** Dedicated mapping methods keep controllers clean and mapping logic testable

The small amount of mapping code required is justified by the significant architectural benefits of loose coupling and API stability.

#### API Structure Organization

To maintain clean separation of concerns and improve code organization, API structures should be extracted from controller files into dedicated companion files following a consistent naming pattern.

**Pattern:** For each `FooController.cs`, create a corresponding `FooApiStructures.cs` file containing all API DTOs specific to that controller.

**File Organization:**
```
ApiHost/Controllers/
├── UserController.cs            // Controller logic only
├── UserApiStructures.cs         // User-specific API DTOs
├── ToDoListController.cs        // Controller logic only  
├── ToDoListApiStructures.cs     // ToDoList-specific API DTOs
└── CommonApiStructures.cs       // Shared API DTOs (e.g., PaginationApiInfo)
```

**Implementation:**
```csharp
// UserApiStructures.cs - Pure API data structures
namespace ApiHost.Controllers;

public sealed record AddUserApiRequest(string Email, string UserName);
public sealed record AddUserApiResponse(string UserId, string Email, string UserName, DateTime CreatedAt);
public sealed record GetUsersApiResponse(IReadOnlyList<UserApiDto> Data, PaginationApiInfo Pagination);

// UserController.cs - Pure orchestration logic
namespace ApiHost.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] AddUserApiRequest request) 
    {
        // Controller focuses solely on HTTP orchestration
    }
}
```

**Benefits:**
- **Single Responsibility:** Controllers contain only HTTP handling logic
- **Reusability:** API structures can be referenced across multiple controllers
- **Maintainability:** Changes to API models don't require editing controller files
- **Organization:** Clear visual separation between data structures and behavior
- **Shared Components:** Common structures are centralized in `CommonApiStructures.cs`

This pattern supports the architectural principle that controllers should be "thin" and focused on their core responsibility of HTTP request orchestration.

## The Composition Root: `ApiHost`

The `ApiHost` project is the only part of the system that is aware of all the modules' implementation details (`Application` layers). It has two critical responsibilities:

1.  **Presentation Layer:** It contains the API Controllers that provide the public HTTP interface for the application. These controllers are thin layers that translate HTTP requests into commands and queries for the bus.
2.  **Configuration Root:** In its `Program.cs` file, it acts as the **Composition Root**. This is where all the application's services are wired together using ASP.NET Core's built-in dependency injection container. It discovers and registers all repositories, message handlers, and services from every module, creating a single, fully configured application ready to run.

### Dependency Injection Pattern

Each module provides `ServiceCollectionExtensions.cs` files in their Application and Infrastructure layers to encapsulate DI registration:

```csharp
// Program.cs uses layer-specific extension methods
builder.Services.AddModuleAInfrastructureServices(connectionString);
builder.Host.AddModuleAApplicationServices();
```

# Database

The database used is Postgresql. There is a single database for all modules. Each module should have its own schema.

For local development, Docker is used to get the database up and running. See [docker-compose.yml](./docker-compose.yml).

# Mediator / In-process message bus

The Wolverine library is used for the Mediator pattern.

Example of dispatching a command:
```csharp
using Wolverine;
// ...

public class SomeClass 
{
    private readonly IMessageBus _messageBus;
    // ...
    
    public async Task SomeMethod(SomeCommandType someCommand) 
    {
        SomeResultType result = await _messageBus.InvokeAsync<SomeResultType>(someCommand);
        // ...
    }
}
```

Example of handling a command:

```csharp
    // here, the first parameter must be the type of the command 
    public static async Task<Result<AddUserResult>> Handle(SomeCommandType command, ISomeDependencyInjectable someService)
    {
        //...
    }
```

Wolverine discovers these kinds of static methods based on the type of the first parameter.
The first parameter must be the type of object sent to the message bus for this discovery to work.

Wolverine dependency-injects rest of the parameters.

The discovery
is enable per-project with configuration in Program.cs:
```csharp
builder.Host.UseWolverine(opts =>
{
    // Auto-discover message handlers
    opts.Discovery.IncludeAssembly(typeof(ModuleA.Application.AssemblyMarker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(ModuleB.Application.AssemblyMarker).Assembly);
    // ...
});
```

## Domain Events Publishing

Domain events are automatically published through the DbContext's `SaveChangesAsync` method using the Wolverine message bus. Here's how the implementation works:

### Implementation Pattern

Each module's DbContext should override `SaveChangesAsync` to:
1. **Collect unpublished domain events** from aggregate roots before saving changes
2. **Save changes** to the database
3. **Publish domain events** only if the database save was successful
4. **Clear domain events** from aggregate roots after successful publishing

### Example Implementation

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    List<IDomainEvent> unpublishedDomainEvents = GetUnpublishedDomainEvents();
    
    int changesSaved = await base.SaveChangesAsync(cancellationToken);
    
    await PublishDomainEventsIfSuccessful(changesSaved, unpublishedDomainEvents);
    
    return changesSaved;
}

private List<IDomainEvent> GetUnpublishedDomainEvents()
{
    return ChangeTracker
        .Entries<AggregateRoot<TId>>()
        .Where(entry => entry.Entity.GetDomainEvents().Count != 0)
        .SelectMany(entry => entry.Entity.GetDomainEvents())
        .ToList();
}

private async Task PublishDomainEventsIfSuccessful(int changesSaved, List<IDomainEvent> domainEvents)
{
    const int NoChanges = 0;
    
    if (changesSaved == NoChanges || _messageBus is null)
        return;
        
    await PublishDomainEvents(domainEvents);
    ClearDomainEventsFromAggregateRoots();
}
```

### Key Principles

- **Transactional Consistency**: Domain events are only published after successful database changes
- **Automatic Collection**: Events are automatically gathered from all tracked aggregate roots
- **Clean State**: Domain events are cleared from aggregates after successful publishing
- **Wolverine Integration**: Events are published through the Wolverine message bus using `PublishAsync`

This pattern ensures that domain events maintain consistency with database changes and are automatically handled without requiring explicit event publishing in command handlers.

## Converting Domain Events to Integration Events

When domain events need to be communicated to other modules, they should be converted to integration events through dedicated handler classes in the Application layer.

### Pattern Implementation

**Domain Event to Integration Event Conversion:**
1. **Domain events** remain internal to the module (e.g., `UserDeletedEvent` in `Users.Domain`)
2. **Integration events** are defined in the module's Contracts project (e.g., `UserDeletedIntegrationEvent` in `Users.Contracts`)
3. **Conversion handlers** in the Application layer transform domain events to integration events

### File Organization

Each module should follow a consistent file naming pattern for integration event handling:

**In `ModuleName.Contracts`:**
- **`IntegrationEvents.cs`** - Single file containing all integration events for the module
  ```csharp
  // Users.Contracts/IntegrationEvents.cs
  namespace Users.Contracts;
  
  public record UserDeletedIntegrationEvent(DateTime OccurredOn, string UserId) : IntegrationEvent(OccurredOn);
  public record UserCreatedIntegrationEvent(DateTime OccurredOn, string UserId, string Email) : IntegrationEvent(OccurredOn);
  ```

**In `ModuleName.Application`:**
- **`DomainToIntegrationEventHandlers.cs`** - Single file containing all domain-to-integration event conversion handlers
  ```csharp
  // Users.Application/DomainToIntegrationEventHandlers.cs
  namespace Users.Application;
  
  public class DomainToIntegrationEventHandlers
  {
      public static async Task Handle(UserDeletedEvent domainEvent, IMessageBus bus)
      {
          UserDeletedIntegrationEvent integrationEvent = new(domainEvent.OccurredOn, domainEvent.UserId.ToString());
          await bus.PublishAsync(integrationEvent);
      }
      
      public static async Task Handle(UserCreatedEvent domainEvent, IMessageBus bus)
      {
          UserCreatedIntegrationEvent integrationEvent = new(domainEvent.OccurredOn, domainEvent.UserId.ToString(), domainEvent.Email.Value);
          await bus.PublishAsync(integrationEvent);
      }
  }
  ```

This naming convention provides:
- **Consistency** across all modules
- **Single source of truth** for integration events per module
- **Easy discoverability** of conversion logic
- **Clear separation** between public contracts and internal conversion handlers

### Key Benefits

- **Encapsulation**: Domain event structure can evolve without affecting other modules
- **Clean Boundaries**: Other modules only see integration events, not internal domain implementation
- **Data Transformation**: Allows different data representations between internal and external contracts
- **Module Independence**: Maintains strict separation between module internals and public contracts

The conversion handlers are automatically discovered by Wolverine through the existing assembly registration mechanism described earlier in this document. This pattern maintains clean architecture boundaries while enabling proper inter-module communication through well-defined integration events.

For AI assistants: To learn more about Wolverine, you can use the context7 mcp server.