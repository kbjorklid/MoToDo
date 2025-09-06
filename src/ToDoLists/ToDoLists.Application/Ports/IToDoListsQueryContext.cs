using Microsoft.EntityFrameworkCore;
using ToDoLists.Domain;

namespace ToDoLists.Application.Ports;

/// <summary>
/// Query context interface for direct database access in CQRS read operations.
/// Provides access to ToDoList entities for optimized querying while maintaining dependency inversion.
/// </summary>
public interface IToDoListsQueryContext
{
    /// <summary>
    /// Gets the ToDoLists DbSet for direct querying.
    /// </summary>
    DbSet<ToDoList> ToDoLists { get; }
}
