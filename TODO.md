# TODO

This file tracks known issues, architectural improvements, and future enhancements for the MoToDo project.

## Race Conditions

### User Deletion During ToDoList Creation

**Problem:** There's a Time-of-Check-Time-of-Use (TOCTOU) race condition in `CreateToDoListCommandHandler`. The handler validates that a user exists before creating a ToDoList, but the user could be deleted between the validation check and when the ToDoList is saved to the database.

**Current Flow:**
1. `CreateToDoListCommandHandler` checks if user exists via `GetUserByIdQuery` ✓
2. User gets deleted and `UserDeletedIntegrationEvent` is processed → existing ToDoLists are deleted
3. Handler saves new ToDoList → orphaned record created with reference to non-existent user

**Impact:** 
- Creates orphaned ToDoLists referencing deleted users
- Violates business invariants
- Could cause downstream errors when querying ToDoLists

**Possible Solutions:**

1. **Saga Pattern (Recommended)** - Use Wolverine Saga to coordinate the multi-step workflow:
   - Stateful coordination tracks progress across validation and creation
   - Can handle `UserDeletedIntegrationEvent` during the process to cancel workflow
   - Atomic completion - entire workflow succeeds or fails
   - Built-in timeout handling

2. **Accept Eventual Consistency** - Document as acceptable business risk:
   - Small time window makes race condition unlikely
   - Implement periodic reconciliation processes
   - Handle gracefully in UI when querying non-existent users

3. **Defensive Re-validation** - Check user existence again just before save:
   - Narrows the race window but doesn't eliminate it
   - Simple to implement but not foolproof

4. **Optimistic Concurrency** - Include user version/timestamp in command:
   - Validate user hasn't changed during processing
   - Requires changes to Users module contract

**Notes:**
- Database foreign key constraints across module schemas would violate modular monolith principles
- Event-driven cleanup alone is insufficient due to timing issues
- The cost of completely eliminating this race condition may outweigh the business impact

**Status:** Open - needs architectural decision on approach