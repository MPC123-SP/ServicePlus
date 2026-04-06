using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class addedAnganWadiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnganWadiDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameOfPost = table.Column<string>(type: "text", nullable: false),
                    PostType = table.Column<int>(type: "integer", nullable: false),
                    CenterCode = table.Column<int>(type: "integer", nullable: false),
                    CenterName = table.Column<string>(type: "text", nullable: false),
                    StatusOfReservation = table.Column<string>(type: "text", nullable: false),
                    VillageLGDCode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnganWadiDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnganWadiDetails");
        }
    }
}
