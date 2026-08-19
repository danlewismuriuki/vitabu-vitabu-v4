using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitabu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddS6Wishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wishlist_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_entries_UserId_CreatedAtUtc",
                table: "wishlist_entries",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_entries_UserId_ListingId",
                table: "wishlist_entries",
                columns: new[] { "UserId", "ListingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wishlist_entries");
        }
    }
}
