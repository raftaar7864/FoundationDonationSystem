using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace FoundationDonationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDonationSlipPerDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonationSlips_DonationId",
                table: "DonationSlips");
            migrationBuilder.CreateIndex(
                name: "IX_DonationSlips_DonationId",
                table: "DonationSlips",
                column: "DonationId",
                unique: true);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonationSlips_DonationId",
                table: "DonationSlips");
            migrationBuilder.CreateIndex(
                name: "IX_DonationSlips_DonationId",
                table: "DonationSlips",
                column: "DonationId");
        }
    }
}
