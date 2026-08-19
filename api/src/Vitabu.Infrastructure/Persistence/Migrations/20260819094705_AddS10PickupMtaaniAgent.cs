using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitabu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddS10PickupMtaaniAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MtaaniAgentId",
                table: "deal_interests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MtaaniAgentName",
                table: "deal_interests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MtaaniEstimatedFeeKes",
                table: "deal_interests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MtaaniLocationId",
                table: "deal_interests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MtaaniLocationName",
                table: "deal_interests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_deal_interests_MtaaniAgentId",
                table: "deal_interests",
                column: "MtaaniAgentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deal_interests_MtaaniAgentId",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "MtaaniAgentId",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "MtaaniAgentName",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "MtaaniEstimatedFeeKes",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "MtaaniLocationId",
                table: "deal_interests");

            migrationBuilder.DropColumn(
                name: "MtaaniLocationName",
                table: "deal_interests");
        }
    }
}
