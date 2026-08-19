using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitabu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddS5CompleteAdminTrust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStaff",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "BuyerCompletedAtUtc",
                table: "deal_interests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeReason",
                table: "deal_interests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisputedAtUtc",
                table: "deal_interests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerCompletedAtUtc",
                table: "deal_interests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "deal_ratings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stars = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deal_ratings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "listing_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deal_ratings_InterestId_FromUserId",
                table: "deal_ratings",
                columns: new[] { "InterestId", "FromUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deal_ratings_ToUserId",
                table: "deal_ratings",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_listing_reports_ListingId",
                table: "listing_reports",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_listing_reports_Status_CreatedAtUtc",
                table: "listing_reports",
                columns: new[] { "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deal_ratings");

            migrationBuilder.DropTable(
                name: "listing_reports");

            migrationBuilder.DropColumn(
                name: "IsStaff",
                table: "users");

            migrationBuilder.DropColumn(
                name: "BuyerCompletedAtUtc",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "DisputeReason",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "DisputedAtUtc",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "SellerCompletedAtUtc",
                table: "deal_interests");
        }
    }
}
