using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitabu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddS8Messaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "message_threads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMessageAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_threads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_message_threads_BuyerUserId_LastMessageAtUtc",
                table: "message_threads",
                columns: new[] { "BuyerUserId", "LastMessageAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_message_threads_ListingId_BuyerUserId",
                table: "message_threads",
                columns: new[] { "ListingId", "BuyerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_threads_SellerUserId_LastMessageAtUtc",
                table: "message_threads",
                columns: new[] { "SellerUserId", "LastMessageAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_messages_ThreadId_CreatedAtUtc",
                table: "messages",
                columns: new[] { "ThreadId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_threads");

            migrationBuilder.DropTable(
                name: "messages");
        }
    }
}
