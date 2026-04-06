using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class updatedCertificateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerCertificateIssued");

            migrationBuilder.CreateTable(
                name: "PlayerCertificateDetails",
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
                    ConveyorName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCertificateDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerIssuedCertificate",
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
                    CertificateSerialNo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerIssuedCertificate", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerCertificateDetails");

            migrationBuilder.DropTable(
                name: "PlayerIssuedCertificate");

            migrationBuilder.CreateTable(
                name: "PlayerCertificateIssued",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicantAgeGroup = table.Column<string>(type: "text", nullable: true),
                    ApplicantDOB = table.Column<string>(type: "text", nullable: true),
                    ApplicantEvent = table.Column<string>(type: "text", nullable: true),
                    ApplicantFatherName = table.Column<string>(type: "text", nullable: true),
                    ApplicantFullName = table.Column<string>(type: "text", nullable: true),
                    ApplicantGame = table.Column<string>(type: "text", nullable: true),
                    ApplicantMobileNo = table.Column<string>(type: "text", nullable: true),
                    CertificateGeneratedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificatePath = table.Column<string>(type: "text", nullable: true),
                    ConveyorName = table.Column<string>(type: "text", nullable: true),
                    GameHeldDistrict = table.Column<string>(type: "text", nullable: true),
                    GameRepresentingDistrict = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<string>(type: "text", nullable: true),
                    TournamentFrom = table.Column<string>(type: "text", nullable: true),
                    TournamentTo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCertificateIssued", x => x.Id);
                });
        }
    }
}
