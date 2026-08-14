using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitabu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAndListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cbc_titles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Grade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Subject = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Term = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MaterialType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Language = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cbc_titles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CbcTitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Grade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Subject = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Term = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    City = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Intent = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Condition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PriceKes = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    InterestCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cbc_titles_Code",
                table: "cbc_titles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cbc_titles_Grade_Subject",
                table: "cbc_titles",
                columns: new[] { "Grade", "Subject" });

            migrationBuilder.CreateIndex(
                name: "IX_listings_CreatedAtUtc",
                table: "listings",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_listings_Status",
                table: "listings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_listings_Status_City_Grade_Subject",
                table: "listings",
                columns: new[] { "Status", "City", "Grade", "Subject" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cbc_titles");

            migrationBuilder.DropTable(
                name: "listings");
        }
    }
}
