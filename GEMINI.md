# GEMINI.md

## Project Overview

**MoToDo** is a .NET 9.0 backend service for task management, built as a **Modular Monolith** using Domain-Driven Design, Clean Architecture, and CQRS patterns. It serves as a comprehensive task management system.

## Documentation Structure

This project maintains detailed documentation across multiple files. Read the appropriate file based on your current task:

- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Deep dive into modular monolith structure, Clean Architecture layers, CQRS implementation, and inter-module communication patterns
- **[CODING_CONVENTIONS.md](./CODING_CONVENTIONS.md)** - DDD implementations, error handling, data mapping, naming conventions, and inheritance rules
- **[REST_CONVENTIONS.md](./REST_CONVENTIONS.md)** - API design standards, versioning, URI conventions, JSON formatting, filtering, sorting, and pagination
- **[TESTING_CONVENTIONS.md](./TESTING_CONVENTIONS.md)** - Testing strategy (test diamond), project organization, system vs unit test guidelines, and Test Object Builder patterns
- **[WOLVERINE.md](devdocs/WOLVERINE.md)** - An index of links to Wolverine's guide for more information about the library.
## Development Commands

### Building and Testing
```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/SystemTests/SystemTests.csproj
dotnet test tests/BuildingBlocks/Base.Domain.Tests/Base.Domain.Tests.csproj

# Run API locally
dotnet run --project src/ApiHost/ApiHost.csproj
```

### Module Management
```powershell
# Create new module with complete structure
.\Add-Module.ps1 -ModuleName "ModuleName"
```

### Database (Local Development)
```bash
# Start PostgreSQL database
docker-compose up -d

# Stop database
docker-compose down

# View database logs
docker-compose logs postgres

# Connect to database
docker exec -it motodo-postgres psql -U postgres -d motodo
```

## Architecture Summary

**Modular Monolith** with each **Module** as a **Bounded Context** from DDD. **Clean Architecture** within modules with dependencies flowing inward: `Domain ← Application ← Infrastructure`. **CQRS** separates writes (Commands) from reads (Queries). Modules communicate only through `Contracts` projects via message bus.

> **For detailed architecture information, see [ARCHITECTURE.md](./ARCHITECTURE.md)**

## Key Conventions Summary

- **Naming**: Strongly typed IDs, suffix conventions for Commands/Queries/Handlers
- **Testing**: Test Diamond strategy with XUnit, NSubstitute, and Test Object Builders
- **Error Handling**: Result pattern, validation exceptions, manual mapping
- **REST**: Versioned APIs, kebab-case URIs, camelCase JSON

## Quick Reference

### Project Dependencies Flow
1. **Domain** → Base.Domain only
2. **Application** → Domain + Contracts + Base.Application + (other module Contracts)
3. **Infrastructure** → Application + Base.Infrastructure
4. **ApiHost** → All module Applications + Contracts
5. **Tests** → Corresponding source projects

### Core Principles
- Entity identifiers must be strongly typed value objects
- Value Objects validate at construction time
- Manual mapping between layers (extension methods/static methods)
- Always use Test Object Builders for test data
- Commands change state, Queries read data - never mix responsibilities
- When the 'Result' pattern is needed, use @src\BuildingBlocks\Base.Domain\Result.cs class
- When Domain objects are created, check if there is a suitable base class or interface in @src\BuildingBlocks\Base.Domain\ , and if yes, derive the class from that.
