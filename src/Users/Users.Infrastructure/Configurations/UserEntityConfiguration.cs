using Base.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain;

namespace Users.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for the User aggregate root, handling value object conversions and database mapping.
/// </summary>
internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "Users");

        // Primary Key - UserId value object
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                guid => UserId.FromGuid(guid)
                    .GetValueOrThrow($"Invalid UserId found in database: {guid}"))
            .HasColumnName("Id");

        // Email value object - store as string
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value.Address,
                emailString => Email.Create(emailString)
                    .GetValueOrThrow($"Invalid Email found in database: {emailString}"))
            .HasColumnName("Email")
            .HasMaxLength(320) // RFC 5321 maximum email length
            .IsRequired();

        // Add unique index on Email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("UQ_Users_Email");

        // UserName value object - store as string  
        builder.Property(u => u.UserName)
            .HasConversion(
                userName => userName.Value,
                userNameString => UserName.Create(userNameString)
                    .GetValueOrThrow($"Invalid UserName found in database: {userNameString}"))
            .HasColumnName("UserName")
            .HasMaxLength(50)
            .IsRequired();

        // Add unique index on UserName
        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("UQ_Users_UserName");

        // Timestamps with timezone support for UTC dates
        builder.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("LastLoginAt")
            .HasColumnType("timestamptz");

        // Optimistic concurrency control with RowVersion
        builder.Property<byte[]>("Version")
            .IsRowVersion()
            .HasColumnName("Version");

        // Performance indexes
        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        builder.HasIndex(u => u.LastLoginAt)
            .HasDatabaseName("IX_Users_LastLoginAt")
            .HasFilter("\"LastLoginAt\" IS NOT NULL");

    }
}
