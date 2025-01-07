using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServicePlusAPIs.AuthenticateModels;
using ServicePlusAPIs.HelperModels;
using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.EnclouserDetails;
using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails;
using ServicePlusAPIs.UserModels;
using System;

namespace ServicePlusAPIs.Context
{
    public class ServicePlusContext : IdentityDbContext<RegisterUser>
    {
        public ServicePlusContext(DbContextOptions<ServicePlusContext> options) : base(options)
        {
            Database.SetCommandTimeout(TimeSpan.FromHours(3));
        }

        public DbSet<InitiatedData> InitiatedDatas { get; set; }
        public DbSet<ExecutionData> ExecutionDatas { get; set; }
        public DbSet<EnclosureDetail> EnclosureDetails { get; set; }
        public DbSet<AttributeDetail> AttributeDetails { get; set; }
        public DbSet<TaskDetail> TaskDetails { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<OfficialFormDetail> OfficialFormDetails { get; set; }
        public DbSet<JSONReceived> JSONReceived { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<ApiNames> ApiNames { get; set; }
        public DbSet<RegisterUserDistrict> RegisterUserDistricts { get; set; }
        public DbSet<RegisterUserDepartment> RegisterUserDepartments { get; set; }
        public DbSet<RegisterUserService> RegisterUserServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary keys for related entities
            modelBuilder.Entity<RegisterUserDistrict>()
                .HasKey(d => d.DistrictId);

            modelBuilder.Entity<RegisterUserDepartment>()
                .HasKey(d => d.DepartmentId);

            modelBuilder.Entity<RegisterUserService>()
                .HasKey(s => s.ServiceId);

            modelBuilder.Entity<EnclosureDetail>()
                .Property(p => p.EnclouserID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<AttributeDetail>()
                .Property(p => p.AttributeDetailID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OfficialFormDetail>()
                .Property(p => p.OfficalFormID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskDetail>()
                .HasOne(ud => ud.UserDetail)
                .WithOne(td => td.TaskDetail)
                .HasForeignKey<UserDetail>(td => td.TaskDetailID);

            // Configure one-to-many relationships between RegisterUser and District, Department, and Service
            modelBuilder.Entity<RegisterUser>()
                .HasMany(u => u.RegisterUserDistricts)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId);

            modelBuilder.Entity<RegisterUser>()
                .HasMany(u => u.RegisterUserDepartments)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId);

            modelBuilder.Entity<RegisterUser>()
                .HasMany(u => u.RegisterUserServices)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId);

            // Call base.OnModelCreating after custom mappings
            base.OnModelCreating(modelBuilder);
        }

    }
}
