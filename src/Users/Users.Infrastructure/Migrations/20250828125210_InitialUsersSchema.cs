using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialUsersSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Users");

        migrationBuilder.CreateTable(
            name: "Users",
            schema: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                LastLoginAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_CreatedAt",
            schema: "Users",
            table: "Users",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Users_LastLoginAt",
            schema: "Users",
            table: "Users",
            column: "LastLoginAt",
            filter: "\"LastLoginAt\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "UQ_Users_Email",
            schema: "Users",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_Users_UserName",
            schema: "Users",
            table: "Users",
            column: "UserName",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users",
            schema: "Users");
    }
}
