using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicCalendarAndSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "calendars",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_visible",
                table: "calendars",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "calendar_subscriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calendar_id = table.Column<long>(type: "bigint", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_calendar_subscriptions_calendars_calendar_id",
                        column: x => x.calendar_id,
                        principalTable: "calendars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_subscriptions_calendar_id",
                table: "calendar_subscriptions",
                column: "calendar_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_subscriptions_user_id_calendar_id",
                table: "calendar_subscriptions",
                columns: new[] { "user_id", "calendar_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_subscriptions");

            migrationBuilder.DropColumn(
                name: "is_public",
                table: "calendars");

            migrationBuilder.DropColumn(
                name: "is_visible",
                table: "calendars");
        }
    }
}
