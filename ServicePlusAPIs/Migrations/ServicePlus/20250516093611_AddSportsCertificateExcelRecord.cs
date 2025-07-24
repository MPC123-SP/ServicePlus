using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class AddSportsCertificateExcelRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportCertificateDetailFromExcels");

            migrationBuilder.CreateTable(
                name: "SportsCertificateExcelRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicantFullName = table.Column<string>(type: "text", nullable: true),
                    ApplicantFatherName = table.Column<string>(type: "text", nullable: true),
                    ApplicantDOB = table.Column<string>(type: "text", nullable: true),
                    ApplicantMobileNo = table.Column<string>(type: "text", nullable: true),
                    GameHeldDistrict = table.Column<string>(type: "text", nullable: true),
                    GameRepresentingDistrict = table.Column<string>(type: "text", nullable: true),
                    ApplicantGame = table.Column<string>(type: "text", nullable: true),
                    TournamentFrom = table.Column<string>(type: "text", nullable: true),
                    TournamentTo = table.Column<string>(type: "text", nullable: true),
                    ApplicantEvent = table.Column<string>(type: "text", nullable: true),
                    ApplicantAgeGroup = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<string>(type: "text", nullable: true),
                    ConveyorName = table.Column<string>(type: "text", nullable: true),
                    CertificateGeneratedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificatePath = table.Column<string>(type: "text", nullable: true),
                    CertificateSerialNo = table.Column<string>(type: "text", nullable: true),
                    ApplicantAgeGroupPB = table.Column<string>(type: "text", nullable: true),
                    ApplicantEventPB = table.Column<string>(type: "text", nullable: true),
                    ApplicantFatherNamePB = table.Column<string>(type: "text", nullable: true),
                    ApplicantFullNamePB = table.Column<string>(type: "text", nullable: true),
                    ApplicantGamePB = table.Column<string>(type: "text", nullable: true),
                    ConveyorNamePB = table.Column<string>(type: "text", nullable: true),
                    GameHeldDistrictPB = table.Column<string>(type: "text", nullable: true),
                    GameRepresentingDistrictPB = table.Column<string>(type: "text", nullable: true),
                    ScorePB = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportsCertificateExcelRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SportsCertificateExcelRecords");

            migrationBuilder.CreateTable(
                name: "ImportCertificateDetailFromExcels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicantAgeGroup = table.Column<string>(type: "text", nullable: true),
                    ApplicantAgeGroupPB = table.Column<string>(type: "text", nullable: true),
                    ApplicantDOB = table.Column<string>(type: "text", nullable: true),
                    ApplicantEvent = table.Column<string>(type: "text", nullable: true),
                    ApplicantEventPB = table.Column<string>(type: "text", nullable: true),
                    ApplicantFatherName = table.Column<string>(type: "text", nullable: true),
                    ApplicantFatherNamePB = table.Column<string>(type: "text", nullable: true),
                    ApplicantFullName = table.Column<string>(type: "text", nullable: true),
                    ApplicantFullNamePB = table.Column<string>(type: "text", nullable: true),
                    ApplicantGame = table.Column<string>(type: "text", nullable: true),
                    ApplicantGamePB = table.Column<string>(type: "text", nullable: true),
                    ApplicantMobileNo = table.Column<string>(type: "text", nullable: true),
                    CertificateGeneratedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificatePath = table.Column<string>(type: "text", nullable: true),
                    CertificateSerialNo = table.Column<string>(type: "text", nullable: true),
                    ConveyorName = table.Column<string>(type: "text", nullable: true),
                    ConveyorNamePB = table.Column<string>(type: "text", nullable: true),
                    GameHeldDistrict = table.Column<string>(type: "text", nullable: true),
                    GameHeldDistrictPB = table.Column<string>(type: "text", nullable: true),
                    GameRepresentingDistrict = table.Column<string>(type: "text", nullable: true),
                    GameRepresentingDistrictPB = table.Column<string>(type: "text", nullable: true),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<string>(type: "text", nullable: true),
                    ScorePB = table.Column<string>(type: "text", nullable: true),
                    TournamentFrom = table.Column<string>(type: "text", nullable: true),
                    TournamentTo = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportCertificateDetailFromExcels", x => x.Id);
                });
        }
    }
}
