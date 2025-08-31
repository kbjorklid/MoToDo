using Base.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoLists.Domain;

namespace ToDoLists.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for the ToDoList aggregate root, handling value object conversions and database mapping.
/// </summary>
internal sealed class ToDoListEntityConfiguration : IEntityTypeConfiguration<ToDoList>
{
    public void Configure(EntityTypeBuilder<ToDoList> builder)
    {
        builder.ToTable("ToDoLists", "ToDoLists", t =>
        {
            // Database-level constraints for data integrity
            t.HasCheckConstraint("CK_ToDoLists_Title_Length",
                $"LENGTH(\"Title\") > 0 AND LENGTH(\"Title\") <= {Title.MaxLength}");

            t.HasCheckConstraint("CK_ToDoLists_UserId_NotEmpty",
                "\"UserId\" != '00000000-0000-0000-0000-000000000000'");

            t.HasCheckConstraint("CK_ToDoLists_Id_NotEmpty",
                "\"Id\" != '00000000-0000-0000-0000-000000000000'");
        });

        // Primary Key - ToDoListId value object
        builder.HasKey(tl => tl.Id);
        builder.Property(tl => tl.Id)
            .HasConversion(
                id => id.Value,
                guid => ToDoListId.FromGuid(guid)
                    .GetValueOrThrow($"Invalid ToDoListId found in database: {guid}"))
            .HasColumnName("Id");

        // UserId value object - store as Guid
        builder.Property(tl => tl.UserId)
            .HasConversion(
                userId => userId.Value,
                guid => UserId.FromGuid(guid)
                    .GetValueOrThrow($"Invalid UserId found in database: {guid}"))
            .HasColumnName("UserId")
            .IsRequired();

        // Title value object - store as string
        builder.Property(tl => tl.Title)
            .HasConversion(
                title => title.Value,
                titleString => Title.Create(titleString)
                    .GetValueOrThrow($"Invalid Title found in database: {titleString}"))
            .HasColumnName("Title")
            .HasMaxLength(Title.MaxLength)
            .IsRequired();

        // Timestamps with timezone support for UTC dates
        builder.Property(tl => tl.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(tl => tl.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasColumnType("timestamptz");

        // Configure owned entities for ToDos
        builder.OwnsMany(tl => tl.Todos, todoBuilder =>
        {
            todoBuilder.ToTable("ToDos", "ToDoLists", t =>
            {
                // Database-level constraints for ToDos data integrity
                t.HasCheckConstraint("CK_ToDos_Title_Length",
                    $"LENGTH(\"Title\") > 0 AND LENGTH(\"Title\") <= {Title.MaxLength}");

                t.HasCheckConstraint("CK_ToDos_Id_NotEmpty",
                    "\"Id\" != '00000000-0000-0000-0000-000000000000'");
            });

            // Primary key for ToDo
            todoBuilder.WithOwner().HasForeignKey("ToDoListId");
            todoBuilder.HasKey("Id", "ToDoListId");

            // ToDoId value object
            todoBuilder.Property(t => t.Id)
                .HasConversion(
                    id => id.Value,
                    guid => ToDoId.FromGuid(guid)
                        .GetValueOrThrow($"Invalid ToDoId found in database: {guid}"))
                .HasColumnName("Id");

            // Title value object
            todoBuilder.Property(t => t.Title)
                .HasConversion(
                    title => title.Value,
                    titleString => Title.Create(titleString)
                        .GetValueOrThrow($"Invalid Title found in database: {titleString}"))
                .HasColumnName("Title")
                .HasMaxLength(Title.MaxLength)
                .IsRequired();

            // Simple properties
            todoBuilder.Property(t => t.IsCompleted)
                .HasColumnName("IsCompleted")
                .IsRequired();

            todoBuilder.Property(t => t.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("timestamptz")
                .IsRequired();

            todoBuilder.Property(t => t.CompletedAt)
                .HasColumnName("CompletedAt")
                .HasColumnType("timestamptz");

            // Indexes for performance
            todoBuilder.HasIndex("ToDoListId", "IsCompleted")
                .HasDatabaseName("IX_ToDos_ToDoListId_IsCompleted");
        });

        // Add indexes for performance and constraints
        builder.HasIndex(tl => tl.UserId)
            .HasDatabaseName("IX_ToDoLists_UserId");

        builder.HasIndex(tl => tl.CreatedAt)
            .HasDatabaseName("IX_ToDoLists_CreatedAt");

        // Optimistic concurrency control with RowVersion
        builder.Property<byte[]>("Version")
            .IsRowVersion()
            .HasColumnName("Version");
    }
}
