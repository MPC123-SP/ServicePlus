using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePlusAPIs.Migrations.PostgresDb
{
    /// <inheritdoc />
    public partial class AddedSports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SportSponsorDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SponsorType = table.Column<string>(type: "text", nullable: false),
                    CashAward = table.Column<string>(type: "text", nullable: true),
                    KindAward = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportSponsorDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SponsorPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplRefNo = table.Column<string>(type: "text", nullable: false),
                    SportSponsorDetailId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorPlayers_SportSponsorDetails_SportSponsorDetailId",
                        column: x => x.SportSponsorDetailId,
                        principalTable: "SportSponsorDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorPlayers_SportSponsorDetailId",
                table: "SponsorPlayers",
                column: "SportSponsorDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SponsorPlayers");

            migrationBuilder.DropTable(
                name: "SportSponsorDetails");
        }
    }
}
