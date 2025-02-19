using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePlusAPIs.Migrations.PostgresDb
{
    /// <inheritdoc />
    public partial class ChangesSports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashAward",
                table: "SportSponsorDetails");

            migrationBuilder.DropColumn(
                name: "KindAward",
                table: "SportSponsorDetails");

            migrationBuilder.DropColumn(
                name: "SponsorType",
                table: "SportSponsorDetails");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SportSponsorDetails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SportSponsorDetails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsStatus",
                table: "SportSponsorDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SportSponsorDetails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CashAward",
                table: "SponsorPlayers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SponsorPlayers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SponsorPlayers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "SponsorPlayers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "SponsorPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStatus",
                table: "SponsorPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KindAward",
                table: "SponsorPlayers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorType",
                table: "SponsorPlayers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SponsorPlayers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SportSponsorDetails");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SportSponsorDetails");

            migrationBuilder.DropColumn(
                name: "IsStatus",
                table: "SportSponsorDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SportSponsorDetails");

            migrationBuilder.DropColumn(
                name: "CashAward",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "IsStatus",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "KindAward",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "SponsorType",
                table: "SponsorPlayers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SponsorPlayers");

            migrationBuilder.AddColumn<string>(
                name: "CashAward",
                table: "SportSponsorDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KindAward",
                table: "SportSponsorDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorType",
                table: "SportSponsorDetails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
