using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace FoundationDonationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddFoundationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "FoundationSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "FoundationSettings",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "FoundationSettings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ReceiptFooter",
                table: "FoundationSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "FoundationSettings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "FoundationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "FoundationName",
                table: "FoundationSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "FoundationSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "FoundationSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FoundationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.AddColumn<string>(
                name: "PAN",
                table: "FoundationSettings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "FoundationSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ThankYouMessage",
                table: "FoundationSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FoundationSettings");
            migrationBuilder.DropColumn(
                name: "PAN",
                table: "FoundationSettings");
            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "FoundationSettings");
            migrationBuilder.DropColumn(
                name: "ThankYouMessage",
                table: "FoundationSettings");
            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "FoundationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ReceiptFooter",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "LogoPath",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "FoundationName",
                table: "FoundationSettings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "FoundationSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
