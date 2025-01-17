﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ServicePlusAPIs.Context;

#nullable disable

namespace ServicePlusAPIs.Migrations.PostgresDb
{
    [DbContext(typeof(PostgresDbContext))]
    [Migration("20230822062346_AllModels")]
    partial class AllModels
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ServicePlusAPIs.HelperModels.CustomLGDDistrict", b =>
                {
                    b.Property<int>("CustomLGDDistrictId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CustomLGDDistrictId"));

                    b.Property<string>("CustomLGDDDistrictCode")
                        .HasColumnType("text");

                    b.Property<string>("CustomLGDDDistrictName")
                        .HasColumnType("text");

                    b.HasKey("CustomLGDDistrictId");

                    b.ToTable("CustomLGDDistricts");
                });

            modelBuilder.Entity("ServicePlusAPIs.HelperModels.CustomLGDTehsilSubTehsil", b =>
                {
                    b.Property<int>("CustomLGDTehsilSubTehsilId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CustomLGDTehsilSubTehsilId"));

                    b.Property<int>("CustomLGDDistrictId")
                        .HasColumnType("integer");

                    b.Property<string>("CustomLGDTehsilSubTehsilCode")
                        .HasColumnType("text");

                    b.Property<string>("CustomLGDTehsilSubTehsilName")
                        .HasColumnType("text");

                    b.HasKey("CustomLGDTehsilSubTehsilId");

                    b.HasIndex("CustomLGDDistrictId");

                    b.ToTable("CustomLGDTehsilSubTehsils");
                });

            modelBuilder.Entity("ServicePlusAPIs.HelperModels.JSONReceived", b =>
                {
                    b.Property<int>("JSONReceivedID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("JSONReceivedID"));

                    b.Property<DateTime>("JsonReceivedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ReceivedExecutionRecord")
                        .HasColumnType("integer");

                    b.Property<int?>("ReceivedInititatedRecord")
                        .HasColumnType("integer");

                    b.HasKey("JSONReceivedID");

                    b.ToTable("JSONReceived");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.AttributeDetail", b =>
                {
                    b.Property<int?>("AttributeDetailID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("AttributeDetailID"));

                    b.Property<string>("ApplicationFormFieldID")
                        .HasColumnType("text");

                    b.Property<string>("ApplicationFormFieldValue")
                        .HasColumnType("text");

                    b.Property<int>("InitiatedDataId")
                        .HasColumnType("integer");

                    b.HasKey("AttributeDetailID");

                    b.HasIndex("InitiatedDataId");

                    b.ToTable("AttributeDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.EnclosureDetail", b =>
                {
                    b.Property<int?>("EnclouserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("EnclouserID"));

                    b.Property<string>("EnclousersId")
                        .HasColumnType("text");

                    b.Property<int?>("EnclousersValue")
                        .HasColumnType("integer");

                    b.Property<int?>("InitiatedDataId")
                        .HasColumnType("integer");

                    b.HasKey("EnclouserID");

                    b.HasIndex("InitiatedDataId");

                    b.ToTable("EnclosureDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.ExecutionData", b =>
                {
                    b.Property<int?>("ExecutionDataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("ExecutionDataId"));

                    b.Property<string>("ApplicantTaskDetails")
                        .HasColumnType("text");

                    b.Property<int?>("ExecutionDataRecordInsertionFlag")
                        .HasColumnType("integer")
                        .HasColumnName("RecordInsertionFlag");

                    b.Property<DateTime?>("ExecutionDataRecordInsertionTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("RecordInsertionTime");

                    b.HasKey("ExecutionDataId");

                    b.ToTable("ExecutionDatas");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.InitiatedData", b =>
                {
                    b.Property<int?>("InitiatedDataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("InitiatedDataId"));

                    b.Property<string>("Amount")
                        .HasColumnType("text");

                    b.Property<int?>("ApplId")
                        .HasColumnType("integer");

                    b.Property<string>("ApplRefNo")
                        .HasColumnType("text");

                    b.Property<string>("AppliedBy")
                        .HasColumnType("text");

                    b.Property<string>("BaseServiceId")
                        .HasColumnType("text");

                    b.Property<string>("DepartmentId")
                        .HasColumnType("text");

                    b.Property<string>("DepartmentName")
                        .HasColumnType("text");

                    b.Property<int?>("InitiatedRecordInsertionFlag")
                        .HasColumnType("integer")
                        .HasColumnName("RecordInsertionFlag");

                    b.Property<DateTime?>("InitiatedRecordInsertionTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("RecordInsertionTime");

                    b.Property<string>("NoOfAttachment")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PaymentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PaymentMode")
                        .HasColumnType("text");

                    b.Property<string>("ReferenceNo")
                        .HasColumnType("text");

                    b.Property<string>("RegistrationId")
                        .HasColumnType("text");

                    b.Property<string>("ServiceId")
                        .HasColumnType("text");

                    b.Property<string>("ServiceName")
                        .HasColumnType("text");

                    b.Property<string>("SubVersion")
                        .HasColumnType("text");

                    b.Property<DateTime?>("SubmissionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SubmissionLocation")
                        .HasColumnType("text");

                    b.Property<string>("SubmissionLocationId")
                        .HasColumnType("text");

                    b.Property<string>("SubmissionLocationTypeId")
                        .HasColumnType("text");

                    b.Property<string>("SubmissionMode")
                        .HasColumnType("text");

                    b.Property<string>("VersionNo")
                        .HasColumnType("text");

                    b.HasKey("InitiatedDataId");

                    b.ToTable("InitiatedDatas");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.OfficialFormDetail", b =>
                {
                    b.Property<int?>("OfficialFormDetailID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("OfficialFormDetailID"));

                    b.Property<int?>("ExecutionDataId")
                        .HasColumnType("integer");

                    b.Property<string>("OfficalFormID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("OfficalFormValue")
                        .HasColumnType("text");

                    b.HasKey("OfficialFormDetailID");

                    b.HasIndex("ExecutionDataId");

                    b.ToTable("OfficialFormDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.TaskDetail", b =>
                {
                    b.Property<int>("TaskDetailID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TaskDetailID"));

                    b.Property<int?>("ActionNo")
                        .HasColumnType("integer");

                    b.Property<string>("ActionTaken")
                        .HasColumnType("text");

                    b.Property<string>("Amount")
                        .HasColumnType("text");

                    b.Property<int?>("ApplId")
                        .HasColumnType("integer");

                    b.Property<string>("CallbackCurrProcId")
                        .HasColumnType("text");

                    b.Property<int?>("CurrentProcessId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ExecutedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ExecutionDataId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("PaymentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PaymentMode")
                        .HasColumnType("text");

                    b.Property<string>("PaymentRefNo")
                        .HasColumnType("text");

                    b.Property<int?>("PullUserId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReceivedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Remarks")
                        .HasColumnType("text");

                    b.Property<int?>("ServiceId")
                        .HasColumnType("integer");

                    b.Property<int?>("TaskId")
                        .HasColumnType("integer");

                    b.Property<string>("TaskName")
                        .HasColumnType("text");

                    b.Property<int?>("TaskType")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("TaskDetailID");

                    b.HasIndex("ExecutionDataId")
                        .IsUnique();

                    b.ToTable("TaskDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.UserDetail", b =>
                {
                    b.Property<int>("UserDetailID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserDetailID"));

                    b.Property<int?>("CurrentProcessId")
                        .HasColumnType("integer");

                    b.Property<string>("DepartmentLevel")
                        .HasColumnType("text");

                    b.Property<string>("Designation")
                        .HasColumnType("text");

                    b.Property<string>("LocationId")
                        .HasColumnType("text");

                    b.Property<string>("LocationName")
                        .HasColumnType("text");

                    b.Property<string>("LocationTypeId")
                        .HasColumnType("text");

                    b.Property<int?>("PullUserId")
                        .HasColumnType("integer");

                    b.Property<int>("TaskDetailID")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("UserDetailID");

                    b.HasIndex("TaskDetailID")
                        .IsUnique();

                    b.ToTable("UserDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ReportsModel.PendencyReport", b =>
                {
                    b.Property<int>("PendencyReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PendencyReportId"));

                    b.Property<int?>("ApplicationRecieved")
                        .HasColumnType("integer");

                    b.Property<int?>("Day1to5")
                        .HasColumnType("integer");

                    b.Property<int?>("Day31to60")
                        .HasColumnType("integer");

                    b.Property<int?>("Day61to90")
                        .HasColumnType("integer");

                    b.Property<int?>("Day6to30")
                        .HasColumnType("integer");

                    b.Property<int?>("Day91toAbove")
                        .HasColumnType("integer");

                    b.Property<int?>("Deliverd")
                        .HasColumnType("integer");

                    b.Property<string>("DistrictName")
                        .HasColumnType("text");

                    b.Property<int?>("InProcess")
                        .HasColumnType("integer");

                    b.Property<double?>("PendencyPercentage")
                        .HasColumnType("double precision");

                    b.Property<int?>("Rejected")
                        .HasColumnType("integer");

                    b.Property<int?>("SendBack")
                        .HasColumnType("integer");

                    b.Property<int?>("TotalPendingDays")
                        .HasColumnType("integer");

                    b.HasKey("PendencyReportId");

                    b.ToTable("PendencyReport");
                });

            modelBuilder.Entity("ServicePlusAPIs.HelperModels.CustomLGDTehsilSubTehsil", b =>
                {
                    b.HasOne("ServicePlusAPIs.HelperModels.CustomLGDDistrict", "CustomLGDDistrict")
                        .WithMany("CustomLGDTehsilSubTehsil")
                        .HasForeignKey("CustomLGDDistrictId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomLGDDistrict");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.AttributeDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.ModelsPos.InitiatedData", "InitiatedData")
                        .WithMany("AttributeDetail")
                        .HasForeignKey("InitiatedDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InitiatedData");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.EnclosureDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.ModelsPos.InitiatedData", "InitiatedData")
                        .WithMany("EnclosureDetails")
                        .HasForeignKey("InitiatedDataId");

                    b.Navigation("InitiatedData");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.OfficialFormDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.ModelsPos.ExecutionData", "ExecutionData")
                        .WithMany("OfficialFormDetail")
                        .HasForeignKey("ExecutionDataId");

                    b.Navigation("ExecutionData");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.TaskDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.ModelsPos.ExecutionData", "ExecutionData")
                        .WithOne("TaskDetails")
                        .HasForeignKey("ServicePlusAPIs.ModelsPos.TaskDetail", "ExecutionDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExecutionData");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.UserDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.ModelsPos.TaskDetail", "TaskDetail")
                        .WithOne("UserDetail")
                        .HasForeignKey("ServicePlusAPIs.ModelsPos.UserDetail", "TaskDetailID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TaskDetail");
                });

            modelBuilder.Entity("ServicePlusAPIs.HelperModels.CustomLGDDistrict", b =>
                {
                    b.Navigation("CustomLGDTehsilSubTehsil");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.ExecutionData", b =>
                {
                    b.Navigation("OfficialFormDetail");

                    b.Navigation("TaskDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.InitiatedData", b =>
                {
                    b.Navigation("AttributeDetail");

                    b.Navigation("EnclosureDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.ModelsPos.TaskDetail", b =>
                {
                    b.Navigation("UserDetail");
                });
#pragma warning restore 612, 618
        }
    }
}
