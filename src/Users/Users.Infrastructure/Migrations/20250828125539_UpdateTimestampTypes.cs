using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateTimestampTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "LastLoginAt",
            schema: "Users",
            table: "Users",
            type: "timestamptz",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            schema: "Users",
            table: "Users",
            type: "timestamptz",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "LastLoginAt",
            schema: "Users",
            table: "Users",
            type: "timestamp",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamptz",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            schema: "Users",
            table: "Users",
            type: "timestamp",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamptz");
    }
}
