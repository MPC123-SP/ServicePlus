using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePlusEXT.Migrations
{
    /// <inheritdoc />
    public partial class add_composite_uniqueness_for_service_applications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceApplications_ApplId",
                table: "ServiceApplications");

            migrationBuilder.DropIndex(
                name: "IX_ServiceApplications_ApplRefNo",
                table: "ServiceApplications");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceApplications_ApplRefNo_ApplId",
                table: "ServiceApplications",
                columns: new[] { "ApplRefNo", "ApplId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceApplications_ApplRefNo_ApplId",
                table: "ServiceApplications");

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
    }
}
