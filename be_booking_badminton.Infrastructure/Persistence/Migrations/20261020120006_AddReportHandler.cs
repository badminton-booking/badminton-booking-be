using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_booking_badminton.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportHandler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "handled_by_user_id",
                table: "reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_reports_handler_status_activity",
                table: "reports",
                columns: new[] { "handled_by_user_id", "status", "last_message_at" },
                descending: new[] { false, false, true },
                filter: "handled_by_user_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.AddCheckConstraint(
                name: "ck_reports_handler",
                table: "reports",
                sql: "(status IN ('OPEN', 'CANCELLED') AND handled_by_user_id IS NULL) OR (status IN ('IN_PROGRESS', 'RESOLVED', 'REJECTED') AND handled_by_user_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "fk_reports_handled_by_user_id",
                table: "reports",
                column: "handled_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_reports_handled_by_user_id",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "ix_reports_handler_status_activity",
                table: "reports");

            migrationBuilder.DropCheckConstraint(
                name: "ck_reports_handler",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "handled_by_user_id",
                table: "reports");
        }
    }
}
