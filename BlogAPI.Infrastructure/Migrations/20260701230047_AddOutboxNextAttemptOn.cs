using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxNextAttemptOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn_OccurredOn",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptOn",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_NextAttemptOn_OccurredOn",
                table: "OutboxMessages",
                columns: new[] { "NextAttemptOn", "OccurredOn" },
                filter: "\"ProcessedOn\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_NextAttemptOn_OccurredOn",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "NextAttemptOn",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_OccurredOn",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "OccurredOn" },
                filter: "\"ProcessedOn\" IS NULL");
        }
    }
}
