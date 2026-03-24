using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePlusEXT.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplRefNo = table.Column<string>(type: "text", nullable: false),
                    ApplId = table.Column<string>(type: "text", nullable: false),
                    AppliedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceApplicationId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationAttribute_ServiceApplications_ServiceApplication~",
                        column: x => x.ServiceApplicationId,
                        principalTable: "ServiceApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAttribute_ServiceApplicationId",
                table: "ApplicationAttribute",
                column: "ServiceApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceApplications_ApplId",
                table: "ServiceApplications",
                column: "ApplId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceApplications_ApplRefNo",
                table: "ServiceApplications",
                column: "ApplRefNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAttribute");

            migrationBuilder.DropTable(
                name: "ServiceApplications");
        }
    }
}
