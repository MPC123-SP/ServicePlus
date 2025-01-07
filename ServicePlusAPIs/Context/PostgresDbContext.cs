using Microsoft.EntityFrameworkCore;
using ServicePlusAPIs.HelperModels;
using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.EnclouserDetails;
using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails;
using ServicePlusAPIs.ReportsModel;

namespace ServicePlusAPIs.Context
{
    public class PostgresDbContext:DbContext
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
        {
            
        }
        public DbSet<InitiatedData> InitiatedDatas { get; set; }
        public DbSet<ExecutionData> ExecutionDatas { get; set; }
        public DbSet<EnclosureDetail> EnclosureDetails { get; set; }
        public DbSet<AttributeDetail> AttributeDetails { get; set; }
        public DbSet<TaskDetail> TaskDetails { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<OfficialFormDetail> OfficialFormDetails { get; set; }
        //public DbSet<MasterService> MasterService { get; set; }
        public DbSet<CustomLGDDistrict> CustomLGDDistricts { get; set; }
        public DbSet<CustomLGDTehsilSubTehsil> CustomLGDTehsilSubTehsils { get; set; }
        public DbSet<PendencyReport> PendencyReport { get; set; }

        public DbSet<JSONReceived> JSONReceived { get; set; }
        public DbSet<CustomAttributeLabel> CustomAttributeLabel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

        }
    }
}
