using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class ChangesinVerifiedCertificat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantAgeGroupPB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantEventPB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantFatherNamePB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantFullNamePB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantGamePB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConveyorNamePB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameHeldDistrictPB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameRepresentingDistrictPB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScorePB",
                table: "PlayerIssuedCertificate",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantAgeGroupPB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "ApplicantEventPB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "ApplicantFatherNamePB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "ApplicantFullNamePB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "ApplicantGamePB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "ConveyorNamePB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "GameHeldDistrictPB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "GameRepresentingDistrictPB",
                table: "PlayerIssuedCertificate");

            migrationBuilder.DropColumn(
                name: "ScorePB",
                table: "PlayerIssuedCertificate");
        }
    }
}
