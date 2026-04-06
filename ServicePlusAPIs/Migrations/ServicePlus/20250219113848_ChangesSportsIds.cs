using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class ChangesSportsIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitiatedDataId",
                table: "SponsorPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatedDataId",
                table: "SponsorPlayers");
        }
    }
}
