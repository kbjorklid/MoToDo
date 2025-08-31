using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoLists.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "ToDoLists");

        migrationBuilder.CreateTable(
            name: "ToDoLists",
            schema: "ToDoLists",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ToDoLists", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ToDos",
            schema: "ToDoLists",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ToDoListId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ToDos", x => new { x.Id, x.ToDoListId });
                table.ForeignKey(
                    name: "FK_ToDos_ToDoLists_ToDoListId",
                    column: x => x.ToDoListId,
                    principalSchema: "ToDoLists",
                    principalTable: "ToDoLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ToDoLists_CreatedAt",
            schema: "ToDoLists",
            table: "ToDoLists",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_ToDoLists_UserId",
            schema: "ToDoLists",
            table: "ToDoLists",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ToDos_ToDoListId_IsCompleted",
            schema: "ToDoLists",
            table: "ToDos",
            columns: new[] { "ToDoListId", "IsCompleted" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ToDos",
            schema: "ToDoLists");

        migrationBuilder.DropTable(
            name: "ToDoLists",
            schema: "ToDoLists");
    }
}
