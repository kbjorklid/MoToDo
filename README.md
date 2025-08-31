# MoToDo

A .NET backend service for task management, built as a modular monolith using Domain-Driven Design, Clean Architecture, and CQRS patterns.

## Overview

MoToDo provides comprehensive task management capabilities:

- **Task creation and management** with comprehensive metadata
- **User management** for task ownership and collaboration
- **Priority and status tracking** for workflow management
- **Clean modular architecture** for maintainability and scalability

## Use Case

MoToDo is a modern task management application built with enterprise-grade patterns and practices. It demonstrates proper implementation of Domain-Driven Design, Clean Architecture, and CQRS in a .NET environment.

## Architecture

MoToDo follows a modular monolith architecture where:

1. **Each module** represents a bounded context from Domain-Driven Design
2. **Clean Architecture** ensures proper dependency flow within each module
3. **CQRS pattern** separates command and query responsibilities
4. **Modules communicate** only through well-defined contracts and message passing

The system is designed for maintainability, testability, and future scalability.
