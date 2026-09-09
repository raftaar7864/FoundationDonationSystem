using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace FoundationDonationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOfflineDonationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOfflineDonation",
                table: "Donations",
                type: "bit",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "OfflineVoucherOriginalName",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "OfflineVoucherPath",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOfflineDonation",
                table: "Donations");
            migrationBuilder.DropColumn(
                name: "OfflineVoucherOriginalName",
                table: "Donations");
            migrationBuilder.DropColumn(
                name: "OfflineVoucherPath",
                table: "Donations");
        }
    }
}
