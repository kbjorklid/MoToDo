using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoLists.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDatabaseConstraints : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddCheckConstraint(
            name: "CK_ToDos_Id_NotEmpty",
            schema: "ToDoLists",
            table: "ToDos",
            sql: "\"Id\" != '00000000-0000-0000-0000-000000000000'");

        migrationBuilder.AddCheckConstraint(
            name: "CK_ToDos_Title_Length",
            schema: "ToDoLists",
            table: "ToDos",
            sql: "LENGTH(\"Title\") > 0 AND LENGTH(\"Title\") <= 200");

        migrationBuilder.AddCheckConstraint(
            name: "CK_ToDoLists_Id_NotEmpty",
            schema: "ToDoLists",
            table: "ToDoLists",
            sql: "\"Id\" != '00000000-0000-0000-0000-000000000000'");

        migrationBuilder.AddCheckConstraint(
            name: "CK_ToDoLists_Title_Length",
            schema: "ToDoLists",
            table: "ToDoLists",
            sql: "LENGTH(\"Title\") > 0 AND LENGTH(\"Title\") <= 200");

        migrationBuilder.AddCheckConstraint(
            name: "CK_ToDoLists_UserId_NotEmpty",
            schema: "ToDoLists",
            table: "ToDoLists",
            sql: "\"UserId\" != '00000000-0000-0000-0000-000000000000'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_ToDos_Id_NotEmpty",
            schema: "ToDoLists",
            table: "ToDos");

        migrationBuilder.DropCheckConstraint(
            name: "CK_ToDos_Title_Length",
            schema: "ToDoLists",
            table: "ToDos");

        migrationBuilder.DropCheckConstraint(
            name: "CK_ToDoLists_Id_NotEmpty",
            schema: "ToDoLists",
            table: "ToDoLists");

        migrationBuilder.DropCheckConstraint(
            name: "CK_ToDoLists_Title_Length",
            schema: "ToDoLists",
            table: "ToDoLists");

        migrationBuilder.DropCheckConstraint(
            name: "CK_ToDoLists_UserId_NotEmpty",
            schema: "ToDoLists",
            table: "ToDoLists");
    }
}
