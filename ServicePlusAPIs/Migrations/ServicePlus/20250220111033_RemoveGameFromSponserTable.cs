using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class RemoveGameFromSponserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Game",
                table: "SponsorPlayers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "SponsorPlayers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
