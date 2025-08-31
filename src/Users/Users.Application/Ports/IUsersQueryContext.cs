using Microsoft.EntityFrameworkCore;
using Users.Domain;

namespace Users.Application.Ports;

/// <summary>
/// Query context interface for direct database access in CQRS read operations.
/// Provides access to User entities for optimized querying while maintaining dependency inversion.
/// </summary>
public interface IUsersQueryContext
{
    /// <summary>
    /// Gets the Users DbSet for direct querying.
    /// </summary>
    DbSet<User> Users { get; }
}
