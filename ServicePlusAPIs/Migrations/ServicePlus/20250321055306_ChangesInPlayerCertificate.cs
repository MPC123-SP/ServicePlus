using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class ChangesInPlayerCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantAgeGroupPB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantEventPB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantFatherNamePB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantFullNamePB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantGamePB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConveyorNamePB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameHeldDistrictPB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameRepresentingDistrictPB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScorePB",
                table: "PlayerCertificateDetails",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantAgeGroupPB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "ApplicantEventPB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "ApplicantFatherNamePB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "ApplicantFullNamePB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "ApplicantGamePB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "ConveyorNamePB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "GameHeldDistrictPB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "GameRepresentingDistrictPB",
                table: "PlayerCertificateDetails");

            migrationBuilder.DropColumn(
                name: "ScorePB",
                table: "PlayerCertificateDetails");
        }
    }
}
