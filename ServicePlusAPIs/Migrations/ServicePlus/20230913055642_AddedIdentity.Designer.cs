﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ServicePlusAPIs.Context;

#nullable disable

namespace ServicePlusAPIs.Migrations.ServicePlus
{
    [DbContext(typeof(ServicePlusContext))]
    [Migration("20230913055642_AddedIdentity")]
    partial class AddedIdentity
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
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

            modelBuilder.Entity("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.TaskDetail", b =>
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

            modelBuilder.Entity("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.UserDetail", b =>
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

            modelBuilder.Entity("ServicePlusAPIs.Models.EnclouserDetails.EnclosureDetail", b =>
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

            modelBuilder.Entity("ServicePlusAPIs.Models.ExecutionModel.ExecutionData", b =>
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

            modelBuilder.Entity("ServicePlusAPIs.Models.InitiatedModel.InitiatedData", b =>
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

            modelBuilder.Entity("ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails.OfficialFormDetail", b =>
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

            modelBuilder.Entity("ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails.AttributeDetail", b =>
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

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.TaskDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.Models.ExecutionModel.ExecutionData", "ExecutionData")
                        .WithOne("TaskDetails")
                        .HasForeignKey("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.TaskDetail", "ExecutionDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExecutionData");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.UserDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.TaskDetail", "TaskDetail")
                        .WithOne("UserDetail")
                        .HasForeignKey("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.UserDetail", "TaskDetailID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TaskDetail");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.EnclouserDetails.EnclosureDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.Models.InitiatedModel.InitiatedData", "InitiatedData")
                        .WithMany("EnclosureDetails")
                        .HasForeignKey("InitiatedDataId");

                    b.Navigation("InitiatedData");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails.OfficialFormDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.Models.ExecutionModel.ExecutionData", "ExecutionData")
                        .WithMany("OfficialFormDetail")
                        .HasForeignKey("ExecutionDataId");

                    b.Navigation("ExecutionData");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails.AttributeDetail", b =>
                {
                    b.HasOne("ServicePlusAPIs.Models.InitiatedModel.InitiatedData", "InitiatedData")
                        .WithMany("AttributeDetail")
                        .HasForeignKey("InitiatedDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InitiatedData");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel.TaskDetail", b =>
                {
                    b.Navigation("UserDetail");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.ExecutionModel.ExecutionData", b =>
                {
                    b.Navigation("OfficialFormDetail");

                    b.Navigation("TaskDetails");
                });

            modelBuilder.Entity("ServicePlusAPIs.Models.InitiatedModel.InitiatedData", b =>
                {
                    b.Navigation("AttributeDetail");

                    b.Navigation("EnclosureDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
