---
description: Review an files that have changes not committed to Git, give the user feedback
argument-hint: Optional instructions
---

## Context

- Current git status: !`git status`
- Recent commits: !`git log --oneline -5`
- Information of the last commit: !`git show --name-status`
- User input: $ARGUMENTS

## Your Task


1. **Identify Review Scope**, use first rule of the following that is applicable:
   1.1 If user has specified the scope of the review, use it
   1.2 If there is previous conversation, deduce the scope from it.
   1.3 If there are uncommitted files, assume the uncommitted changes are the scope of the review.

2. **Read Project Documentation**:
    - CODING_CONVENTIONS.md
    - ARCHITECTURE.md
    - REST_CONVENTIONS.md if reviewing REST API controllers or endpoints
    - TESTING_CONVENTIONS.md if reviewing automated unit or system tests

3. **Conduct Comprehensive Analysis**: Examine the code through multiple lenses:
    - **Architectural Compliance**: Verify adherence to Clean Architecture principles, proper dependency flow, and modular boundaries
    - **DDD Implementation**: Check domain model design, aggregate boundaries, value objects, entities, and domain events
    - **CQRS Patterns**: Ensure proper separation of commands and queries, handler implementations
    - **Code Quality**: Assess naming conventions, SOLID principles, error handling, and maintainability
    - **Project Standards**: Verify compliance with established coding conventions, testing patterns, and REST API standards

4. **Identify possilbe easy to miss problems** such as
    - Possible race conditions
    - Possible deadlock scenarios
    - N+1 query problems

4. **Apply Domain Expertise**: Draw from your knowledge of:
    - .NET 9.0 best practices and modern C# features
    - Entity Framework and database design patterns
    - Dependency injection and service registration
    - Result patterns and error handling strategies
    - Test-driven development and testing strategies

5. **Be Specific and Actionable**: Provide concrete examples and specific recommendations rather than generic advice. Reference relevant design patterns, architectural principles, or project conventions when applicable.

6. **Consider Project Context**: Take into account the modular monolith architecture, existing patterns, and established conventions when making recommendations.

7. **Provide Structured Feedback** in exactly this format:

   **CRITICAL ISSUES** (Must Fix):
    - List any architectural violations, security concerns, or bugs that must be addressed
    - Include specific file/line references where possible
    - Explain the impact of each issue

   **MINOR ISSUES** (Consider Fixing):
    - List minor violations to coding conventions
    - Include specific file/line references where possible

   **SUGGESTED IMPROVEMENTS** (Should Consider):
    - Suggest architectural enhancements and design pattern opportunities
    - Recommend refactoring for better maintainability
    - Identify missing tests or documentation
    - Propose performance optimizations
    - Suggest adherence to project conventions

Use numbers for each fix/improvement suggestion so that user may refer to them (ask for fixing).

After listing the suggestions, ask:
"Would you like me to fix any of the findings? You can list the numbers of the items you'd like me to fix"

## Conclusion

Your goal is to elevate code quality while ensuring architectural consistency and maintainability.
Focus on both immediate fixes and strategic improvements that align with the project's long-term architectural vision.
