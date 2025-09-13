using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiItemSuggestions.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "AiItemSuggestions");

        migrationBuilder.CreateTable(
            name: "ToDoListSuggestions",
            schema: "AiItemSuggestions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ToDoListId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                LastSuggestionAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ToDoListSuggestions", x => x.Id);
                table.CheckConstraint("CK_ToDoListSuggestions_Id_NotEmpty", "\"Id\" != '00000000-0000-0000-0000-000000000000'");
                table.CheckConstraint("CK_ToDoListSuggestions_ToDoListId_NotEmpty", "\"ToDoListId\" != '00000000-0000-0000-0000-000000000000'");
            });

        migrationBuilder.CreateTable(
            name: "SuggestedItems",
            schema: "AiItemSuggestions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ToDoListSuggestionsId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CorrespondingToDoId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SuggestedItems", x => new { x.Id, x.ToDoListSuggestionsId });
                table.CheckConstraint("CK_SuggestedItems_CorrespondingToDoId_NotEmpty", "\"CorrespondingToDoId\" != '00000000-0000-0000-0000-000000000000'");
                table.CheckConstraint("CK_SuggestedItems_Id_NotEmpty", "\"Id\" != '00000000-0000-0000-0000-000000000000'");
                table.CheckConstraint("CK_SuggestedItems_Title_Length", "LENGTH(\"Title\") >= 3 AND LENGTH(\"Title\") <= 200");
                table.ForeignKey(
                    name: "FK_SuggestedItems_ToDoListSuggestions_ToDoListSuggestionsId",
                    column: x => x.ToDoListSuggestionsId,
                    principalSchema: "AiItemSuggestions",
                    principalTable: "ToDoListSuggestions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SuggestedItems_CorrespondingToDoId",
            schema: "AiItemSuggestions",
            table: "SuggestedItems",
            column: "CorrespondingToDoId");

        migrationBuilder.CreateIndex(
            name: "IX_SuggestedItems_ToDoListSuggestionsId_CorrespondingToDoId",
            schema: "AiItemSuggestions",
            table: "SuggestedItems",
            columns: new[] { "ToDoListSuggestionsId", "CorrespondingToDoId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ToDoListSuggestions_CreatedAt",
            schema: "AiItemSuggestions",
            table: "ToDoListSuggestions",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_ToDoListSuggestions_LastSuggestionAt",
            schema: "AiItemSuggestions",
            table: "ToDoListSuggestions",
            column: "LastSuggestionAt");

        migrationBuilder.CreateIndex(
            name: "IX_ToDoListSuggestions_ToDoListId",
            schema: "AiItemSuggestions",
            table: "ToDoListSuggestions",
            column: "ToDoListId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SuggestedItems",
            schema: "AiItemSuggestions");

        migrationBuilder.DropTable(
            name: "ToDoListSuggestions",
            schema: "AiItemSuggestions");
    }
}
