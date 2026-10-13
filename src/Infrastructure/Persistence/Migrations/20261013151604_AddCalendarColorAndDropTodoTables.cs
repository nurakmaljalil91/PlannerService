using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarColorAndDropTodoTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todo_items");

            migrationBuilder.DropTable(
                name: "todo_lists");

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "calendars",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                table: "calendars");

            migrationBuilder.CreateTable(
                name: "todo_lists",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    colour_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_todo_lists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "todo_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    list_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    reminder = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_todo_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_todo_items_todo_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "todo_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_todo_items_list_id",
                table: "todo_items",
                column: "list_id");
        }
    }
}
