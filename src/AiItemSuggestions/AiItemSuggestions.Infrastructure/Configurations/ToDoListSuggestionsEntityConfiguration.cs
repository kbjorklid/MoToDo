using AiItemSuggestions.Domain;
using Base.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiItemSuggestions.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for the ToDoListSuggestions aggregate root, handling value object conversions and database mapping.
/// </summary>
internal sealed class ToDoListSuggestionsEntityConfiguration : IEntityTypeConfiguration<ToDoListSuggestions>
{
    public void Configure(EntityTypeBuilder<ToDoListSuggestions> builder)
    {
        builder.ToTable("ToDoListSuggestions", "AiItemSuggestions", t =>
        {
            // Database-level constraints for data integrity
            t.HasCheckConstraint("CK_ToDoListSuggestions_ToDoListId_NotEmpty",
                "\"ToDoListId\" != '00000000-0000-0000-0000-000000000000'");

            t.HasCheckConstraint("CK_ToDoListSuggestions_Id_NotEmpty",
                "\"Id\" != '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasKey(tls => tls.Id);
        builder.Property(tls => tls.Id)
            .HasConversion(
                id => id.Value,
                guid => ToDoListSuggestionsId.FromGuid(guid)
                    .GetValueOrThrow($"Invalid ToDoListSuggestionsId found in database: {guid}"))
            .HasColumnName("Id");

        builder.Property(tls => tls.ToDoListId)
            .HasConversion(
                toDoListId => toDoListId.Value,
                guid => ToDoListId.FromGuid(guid)
                    .GetValueOrThrow($"Invalid ToDoListId found in database: {guid}"))
            .HasColumnName("ToDoListId")
            .IsRequired();

        builder.Property(tls => tls.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(tls => tls.LastSuggestionAt)
            .HasColumnName("LastSuggestionAt")
            .HasColumnType("timestamptz");

        // Configure owned entities for SuggestedItems
        builder.OwnsMany(tls => tls.SuggestedItems, suggestedItemBuilder =>
        {
            suggestedItemBuilder.ToTable("SuggestedItems", "AiItemSuggestions", t =>
            {
                // Database-level constraints for SuggestedItems data integrity
                t.HasCheckConstraint("CK_SuggestedItems_Title_Length",
                    $"LENGTH(\"Title\") >= {SuggestedItemTitle.MinLength} AND LENGTH(\"Title\") <= {SuggestedItemTitle.MaxLength}");

                t.HasCheckConstraint("CK_SuggestedItems_Id_NotEmpty",
                    "\"Id\" != '00000000-0000-0000-0000-000000000000'");

                t.HasCheckConstraint("CK_SuggestedItems_CorrespondingToDoId_NotEmpty",
                    "\"CorrespondingToDoId\" != '00000000-0000-0000-0000-000000000000'");
            });

            // Primary key for SuggestedItem
            suggestedItemBuilder.WithOwner().HasForeignKey("ToDoListSuggestionsId");
            suggestedItemBuilder.HasKey("Id", "ToDoListSuggestionsId");

            suggestedItemBuilder.Property(si => si.Id)
                .HasConversion(
                    id => id.Value,
                    guid => SuggestedItemId.FromGuid(guid)
                        .GetValueOrThrow($"Invalid SuggestedItemId found in database: {guid}"))
                .HasColumnName("Id");

            suggestedItemBuilder.Property(si => si.Title)
                .HasConversion(
                    title => title.Value,
                    titleString => SuggestedItemTitle.Create(titleString)
                        .GetValueOrThrow($"Invalid SuggestedItemTitle found in database: {titleString}"))
                .HasColumnName("Title")
                .HasMaxLength(SuggestedItemTitle.MaxLength)
                .IsRequired();

            suggestedItemBuilder.Property(si => si.CorrespondingToDoId)
                .HasConversion(
                    toDoId => toDoId.Value,
                    guid => ToDoId.FromGuid(guid)
                        .GetValueOrThrow($"Invalid ToDoId found in database: {guid}"))
                .HasColumnName("CorrespondingToDoId")
                .IsRequired();

            suggestedItemBuilder.Property(si => si.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("timestamptz")
                .IsRequired();

            // Indexes for performance
            suggestedItemBuilder.HasIndex("ToDoListSuggestionsId", "CorrespondingToDoId")
                .HasDatabaseName("IX_SuggestedItems_ToDoListSuggestionsId_CorrespondingToDoId")
                .IsUnique(); // Ensure one suggested item per todo item per suggestions list

            suggestedItemBuilder.HasIndex("CorrespondingToDoId")
                .HasDatabaseName("IX_SuggestedItems_CorrespondingToDoId");
        });

        // Add indexes for performance and constraints
        builder.HasIndex(tls => tls.ToDoListId)
            .HasDatabaseName("IX_ToDoListSuggestions_ToDoListId")
            .IsUnique(); // Ensure one suggestions record per todo list

        builder.HasIndex(tls => tls.CreatedAt)
            .HasDatabaseName("IX_ToDoListSuggestions_CreatedAt");

        builder.HasIndex(tls => tls.LastSuggestionAt)
            .HasDatabaseName("IX_ToDoListSuggestions_LastSuggestionAt");

        // Optimistic concurrency control with RowVersion
        builder.Property<byte[]>("Version")
            .IsRowVersion()
            .HasColumnName("Version");
    }
}
