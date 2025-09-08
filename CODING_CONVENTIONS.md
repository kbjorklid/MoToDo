# DDD class implementations

## Value Objects

- Value Objects must validate data at construction time.
- Consider making simple value objects `struct`.
- Consider using `record`.

## Entities

### Entity Identifiers

Do not use primitive types or common library types such ad `Guid` as entity IDs. Instead, create a new value type 
or reference type that wraps the common data type. The aim is to strongly type the Ids.

Example of a `Guid`-valued Entity ID:

```csharp
public readonly record struct UserId(Guid Value)
{
    public static class Codes
    {
        public const string Empty = "UserId.Empty";
    }

    public static UserId New() => new(Guid.NewGuid());
    public static Result<UserId> FromGuid(Guid value)
    {
        return (value == Guid.Empty)
            ? new Error(Codes.Empty, "UserId GUID cannot represent an empty value.", ErrorType.Validation)
            : new UserId(value);
    }
    public static Result<UserId> FromString(string value)
    {
        try
        {
            return FromGuid(Guid.Parse(value));
        }
        catch (FormatException)
        {
            return new Error(Codes.GuidFormat, $"Invalid Guid format: {value}.", ErrorType.Validation);
        }
    }
    public static implicit operator Guid(UserId userId) => userId.Value;
    public override string ToString() => Value.ToString();
}
```

If the value type is not an unique identifier (such as `Guid`) that can be created on the fly, then the `New()` method should be left out.

### Current time

Entity methods requiring current time should take it as a parameter as opposed to using System.CurrentTime.
This is for testability
```csharp
    // Good
    public static User Register(Email email, UserName userName, DateTime createdAt)
    {
        // ...
        var user = new User(userId, email, userName, createdAt);
        // ...
    }

    // Avoid
    public static User Register(Email email, UserName userName)
    {
        // ...
        DateTime createdAt = DateTime.UtcNow;
        var user = new User(userId, email, userName, createdAt);
        // ...
    }
```

# Result pattern (instead of Exceptions)

Use the Result pattern. The Result class(es) can be found here: `src/BuildingBlocks/Result.cs`.

For the strings used as the `Code` argument of an `Error` instance, create constants to the class that
creates the errors, example:
```csharp
public readonly record struct Email {

    public static class Codes {
        public const string Empty = "Email.Empty";
        public const string InvalidFormat = "Email.InvalidFormat";
    }

    public static Result<Email> New(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            return new Error(Codes.Empty, "Email address cannot be empty.");
        // ...
    }
}
```

Use implicit operators to simplify code:
```csharp
// Avoid
return Result<User>.Success(user);

// Good
return user;
```

```csharp
// Avoid
return Result.Failure(Codes.Empty, "Email address cannot be empty.")

// Good
return new Error(Codes.Empty, "Email address cannot be empty.");
```

# Data mapping between Entities, DTOs, etc.

- Use manual mapping, e.g. extension methods or static methods. Do not use AutoMapper or similar.

# Code Structure and Readability

## Extract Inline Logic to Well-Named Methods

When you have complex inline logic or multiple operations within a method, extract them into well-named private methods to improve readability and maintainability.

### Guidelines

- Replace inline conditional logic and string manipulation with descriptive method calls
- Use method names that clearly express the intent and return type
- Keep the main method flow readable by focusing on the "what" rather than the "how"
- Extract logic even for simple operations if it improves clarity
- The extracted private method should be placed under the first method that calls it.

### Example

```csharp
// Before: Inline logic obscures the main flow
public Result<(TEnum, bool)> Parse(string sort)
{
    if (string.IsNullOrWhiteSpace(sort))
        return (_defaultSortBy, _defaultAscending);

    bool ascending = true;
    string sortField = sort;

    // Check if sort parameter starts with '-' for descending order
    if (sort.StartsWith('-'))
    {
        ascending = false;
        sortField = sort[1..]; // Remove the '-' prefix
    }

    // ... rest of method
}

// After: Extracted logic makes intent clear
public Result<(TEnum, bool)> Parse(string sort)
{
    if (string.IsNullOrWhiteSpace(sort))
        return (_defaultSortBy, _defaultAscending);

    bool ascending = IsAscending(sort);
    string sortField = SortFieldName(sort);
    
    // ... rest of method
}

private static bool IsAscending(string sort) => !sort.StartsWith('-');

private static string SortFieldName(string sort) => IsAscending(sort) ? sort : sort[1..];
```

### Benefits

- **Self-documenting code**: Method names replace the need for comments
- **Easier testing**: Small methods can be tested independently if needed
- **Reduced cognitive load**: Main method focuses on business logic flow
- **Reusability**: Extracted methods can be reused elsewhere if needed

# Repository Query Patterns

## QueryCriteria Pattern

For complex repository queries with multiple optional parameters (filters, sorting, paging), use the QueryCriteria pattern with a fluent builder API.

### Structure
- `readonly record struct` with validation constants in nested `Codes` class
- Private constructor, public fluent builder via static `Builder()` method
- Builder has `WithXxx()` methods returning `this`, `Build()` returns `Result<QueryCriteria>`
- Validation in private `Create()` method shared by builder

### Key Implementation Points
```csharp
public readonly record struct UserQueryCriteria
{
    public static class Codes { public const string EmptyEmailFilter = "UserQueryCriteria.EmptyEmailFilter"; }
    
    public string? EmailFilter { get; }
    // ... other properties
    
    private UserQueryCriteria(...) { /* assign properties */ }
    
    public static UserQueryCriteriaBuilder Builder(PagingParameters pagingParameters) => new(pagingParameters);
    
    public class UserQueryCriteriaBuilder
    {
        public UserQueryCriteriaBuilder WithEmailFilter(string emailFilter) { _emailFilter = emailFilter; return this; }
        public UserQueryCriteriaBuilder Descending() { _ascending = false; return this; }
        public Result<UserQueryCriteria> Build() => Create(_pagingParameters, _emailFilter, ...);
    }
    
    private static Result<UserQueryCriteria> Create(...) 
    {
        // Validation logic with Result pattern
        if (emailFilter is not null && string.IsNullOrWhiteSpace(emailFilter))
            return new Error(Codes.EmptyEmailFilter, "...", ErrorType.Validation);
        return new UserQueryCriteria(...);
    }
}
```

### Repository Usage
```csharp
public interface IUserRepository
{
    Task<PagedResult<User>> FindUsersAsync(UserQueryCriteria criteria, CancellationToken cancellationToken = default);
}

// Usage
var criteriaResult = UserQueryCriteria.Builder(pagingParams)
    .WithEmailFilter("@company.com").WithSortBy(UsersSortBy.Email).Descending().Build();
```

### When to Use
Multiple optional parameters (3+), complex validation, reusable across repository methods, type safety over primitives.

# Naming Conventions

## Module vs Class Naming

- **Modules (directories and projects)** MUST use **plural** names (e.g., `Users`, `ToDoLists`)
- **Classes** MUST use **singular** names (e.g., `UserController`, `UserRepository`, `ToDoListService`)

**Examples:**
- Module: `Users` directory with `Users.Domain.csproj`
- Classes: `UserController`, `UserRepository`, `UserService`
- Module: `ToDoLists` directory with `ToDoLists.Application.csproj`
- Classes: `ToDoListController`, `ToDoListRepository`, `ToDoListService`

## Class Naming Conventions

- Interfaces MUST start with `I` prefix.
- Controllers MUST end with `Controller` suffix.
- Exceptions MUST end with `Exception` suffix.
- Command Handler classes MUST end with `CommandHandler` suffix.
- Query Handler classes MUST end with `QueryHandler` suffix.
- Commands MUST end with `Command` suffix.
- Queries MUST end with `Query` suffix.
- Domain Events MUST end with `Event` suffix.
- Domain Services MUST end with `Service` suffix.
- Repository interfaces MUST end with `Repository` suffix.
- Repository implementations MUST end with `Repository` suffix.
- Port interfaces MUST end with `Port` Suffix.
- Adapter implementations (that implement Port interfaces) MUST end with `Adapter` suffix.
- .cs file names should be same as the type defined within them (e.g. `UserRegisteredEvent` -> `UserRegisteredEvent.cs`)

# Inheritance Conventions

- Entities should inherit from the [Entity<TId>](./src/BuildingBlocks/Base.Domain/Entity.cs) abstract class.
- Aggregate Roots should inherit from the [AggregateRoot<TId>](./src/BuildingBlocks/Base.Domain/AggregateRoot.cs) abstract class.
- Domain Events MUST implement the [IDomainEvent](./src/BuildingBlocks/Base.Domain/IDomainEvent.cs) interface.
- Commands and Queries should be implemented as immutable `record` types.
- Repository interfaces should be defined in the Domain layer and implemented in the Infrastructure layer.

# Code documentation

- Add XML documentation for classes
- Avoid adding documentation that is redundant given the class/type/method name
- DO NOT add XML documentation to methods or properties. Some exceptions:
  - Do document methods that modify data in Aggregate Roots and Entities
  - Do document methods of Domain Services
- DO NOT add inline (`//`) comments. This includes step-by-step explanatory comments (e.g., `// Parse and validate the userId`, `// Get the user`, `// Save changes`). The code should be self-explanatory through clear method and variable names. Exceptions:
  - Do add `// Arrange`, `// Act` and `// Assert` comments to tests.
- NEVER remove useful pre-existing documentation, even when it is against the guidelines above.

# Important base and utility classes

- [AggregateRoot](./src/BuildingBlocks/Base.Domain/AggregateRoot.cs): Base class for aggregate roots
- [Entity](./src/BuildingBlocks/Base.Domain/Entity.cs): Base class for all entities in the domain
- [IDomainEvent](./src/BuildingBlocks/Base.Domain/IDomainEvent.cs): Interface for domain events that communicate changes within and between aggregates
- [DomainException](./src/BuildingBlocks/Base.Domain/DomainException.cs): Base exception for domain rule violations and business invariant failures
- [Result](./src/BuildingBlocks/Base.Domain/Result/Result.cs): Result pattern implementation for error handling without exceptions
- [Error](./src/BuildingBlocks/Base.Domain/Result/Error.cs): Structured error type with code, description, and type for Result pattern
- [ErrorType](./src/BuildingBlocks/Base.Domain/Result/ErrorType.cs): Enumeration defining types of errors (Failure, Validation, NotFound, Unexpected)
- [PagedResult](./src/BuildingBlocks/Base.Domain/PagedResult.cs): Generic container for paginated data with metadata
- [PagingParameters](./src/BuildingBlocks/Base.Domain/PagingParameters.cs): Value object for pagination parameters with validation

# Other documentation

- [src/ApiHost/PROJECT_CONVENTIONS.md](./src/ApiHost/PROJECT_CONVENTIONS.md) - read this if you modify or create
  Controllers, or edit any files in the ApiHost project.