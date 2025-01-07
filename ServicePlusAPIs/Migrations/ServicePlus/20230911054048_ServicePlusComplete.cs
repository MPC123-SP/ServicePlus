using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    /// <inheritdoc />
    public partial class ServicePlusComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutionDatas",
                columns: table => new
                {
                    ExecutionDataId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicantTaskDetails = table.Column<string>(type: "text", nullable: true),
                    RecordInsertionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordInsertionFlag = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionDatas", x => x.ExecutionDataId);
                });

            migrationBuilder.CreateTable(
                name: "InitiatedDatas",
                columns: table => new
                {
                    InitiatedDataId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<string>(type: "text", nullable: true),
                    DepartmentName = table.Column<string>(type: "text", nullable: true),
                    ServiceId = table.Column<string>(type: "text", nullable: true),
                    ServiceName = table.Column<string>(type: "text", nullable: true),
                    ApplId = table.Column<int>(type: "integer", nullable: true),
                    ApplRefNo = table.Column<string>(type: "text", nullable: true),
                    NoOfAttachment = table.Column<string>(type: "text", nullable: true),
                    SubmissionMode = table.Column<string>(type: "text", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedBy = table.Column<string>(type: "text", nullable: true),
                    SubmissionLocation = table.Column<string>(type: "text", nullable: true),
                    SubmissionLocationId = table.Column<string>(type: "text", nullable: true),
                    SubmissionLocationTypeId = table.Column<string>(type: "text", nullable: true),
                    PaymentMode = table.Column<string>(type: "text", nullable: true),
                    ReferenceNo = table.Column<string>(type: "text", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Amount = table.Column<string>(type: "text", nullable: true),
                    RegistrationId = table.Column<string>(type: "text", nullable: true),
                    BaseServiceId = table.Column<string>(type: "text", nullable: true),
                    VersionNo = table.Column<string>(type: "text", nullable: true),
                    SubVersion = table.Column<string>(type: "text", nullable: true),
                    RecordInsertionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordInsertionFlag = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitiatedDatas", x => x.InitiatedDataId);
                });

            migrationBuilder.CreateTable(
                name: "JSONReceived",
                columns: table => new
                {
                    JSONReceivedID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JsonReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedInititatedRecord = table.Column<int>(type: "integer", nullable: true),
                    ReceivedExecutionRecord = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JSONReceived", x => x.JSONReceivedID);
                });

            migrationBuilder.CreateTable(
                name: "OfficialFormDetails",
                columns: table => new
                {
                    OfficialFormDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExecutionDataId = table.Column<int>(type: "integer", nullable: true),
                    OfficalFormID = table.Column<string>(type: "text", nullable: true),
                    OfficalFormValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficialFormDetails", x => x.OfficialFormDetailID);
                    table.ForeignKey(
                        name: "FK_OfficialFormDetails_ExecutionDatas_ExecutionDataId",
                        column: x => x.ExecutionDataId,
                        principalTable: "ExecutionDatas",
                        principalColumn: "ExecutionDataId");
                });

            migrationBuilder.CreateTable(
                name: "TaskDetails",
                columns: table => new
                {
                    TaskDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExecutionDataId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<string>(type: "text", nullable: true),
                    ApplId = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    TaskId = table.Column<int>(type: "integer", nullable: true),
                    ActionNo = table.Column<int>(type: "integer", nullable: true),
                    TaskName = table.Column<string>(type: "text", nullable: true),
                    TaskType = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    ServiceId = table.Column<int>(type: "integer", nullable: true),
                    ActionTaken = table.Column<string>(type: "text", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentMode = table.Column<string>(type: "text", nullable: true),
                    PullUserId = table.Column<int>(type: "integer", nullable: true),
                    ExecutedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentRefNo = table.Column<string>(type: "text", nullable: true),
                    CurrentProcessId = table.Column<int>(type: "integer", nullable: true),
                    CallbackCurrProcId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDetails", x => x.TaskDetailID);
                    table.ForeignKey(
                        name: "FK_TaskDetails_ExecutionDatas_ExecutionDataId",
                        column: x => x.ExecutionDataId,
                        principalTable: "ExecutionDatas",
                        principalColumn: "ExecutionDataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeDetails",
                columns: table => new
                {
                    AttributeDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InitiatedDataId = table.Column<int>(type: "integer", nullable: false),
                    ApplicationFormFieldID = table.Column<string>(type: "text", nullable: true),
                    ApplicationFormFieldValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDetails", x => x.AttributeDetailID);
                    table.ForeignKey(
                        name: "FK_AttributeDetails_InitiatedDatas_InitiatedDataId",
                        column: x => x.InitiatedDataId,
                        principalTable: "InitiatedDatas",
                        principalColumn: "InitiatedDataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnclosureDetails",
                columns: table => new
                {
                    EnclouserID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InitiatedDataId = table.Column<int>(type: "integer", nullable: true),
                    EnclousersId = table.Column<string>(type: "text", nullable: true),
                    EnclousersValue = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnclosureDetails", x => x.EnclouserID);
                    table.ForeignKey(
                        name: "FK_EnclosureDetails_InitiatedDatas_InitiatedDataId",
                        column: x => x.InitiatedDataId,
                        principalTable: "InitiatedDatas",
                        principalColumn: "InitiatedDataId");
                });

            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    UserDetailID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskDetailID = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Designation = table.Column<string>(type: "text", nullable: true),
                    LocationId = table.Column<string>(type: "text", nullable: true),
                    PullUserId = table.Column<int>(type: "integer", nullable: true),
                    LocationName = table.Column<string>(type: "text", nullable: true),
                    DepartmentLevel = table.Column<string>(type: "text", nullable: true),
                    LocationTypeId = table.Column<string>(type: "text", nullable: true),
                    CurrentProcessId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.UserDetailID);
                    table.ForeignKey(
                        name: "FK_UserDetails_TaskDetails_TaskDetailID",
                        column: x => x.TaskDetailID,
                        principalTable: "TaskDetails",
                        principalColumn: "TaskDetailID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDetails_InitiatedDataId",
                table: "AttributeDetails",
                column: "InitiatedDataId");

            migrationBuilder.CreateIndex(
                name: "IX_EnclosureDetails_InitiatedDataId",
                table: "EnclosureDetails",
                column: "InitiatedDataId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficialFormDetails_ExecutionDataId",
                table: "OfficialFormDetails",
                column: "ExecutionDataId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDetails_ExecutionDataId",
                table: "TaskDetails",
                column: "ExecutionDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_TaskDetailID",
                table: "UserDetails",
                column: "TaskDetailID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttributeDetails");

            migrationBuilder.DropTable(
                name: "EnclosureDetails");

            migrationBuilder.DropTable(
                name: "JSONReceived");

            migrationBuilder.DropTable(
                name: "OfficialFormDetails");

            migrationBuilder.DropTable(
                name: "UserDetails");

            migrationBuilder.DropTable(
                name: "InitiatedDatas");

            migrationBuilder.DropTable(
                name: "TaskDetails");

            migrationBuilder.DropTable(
                name: "ExecutionDatas");
        }
    }
}
