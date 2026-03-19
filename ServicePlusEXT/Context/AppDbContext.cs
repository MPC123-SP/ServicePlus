

using Microsoft.EntityFrameworkCore;
using ServicePlusEXT.Entities;

namespace ServicePlusEXT.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ServiceApplication> ServiceApplications { get; set; }
 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceApplication>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ApplicationId).IsRequired().HasMaxLength(100);

                entity.Property(x => x.ApplicantName).HasMaxLength(200);

                entity.Property(x => x.Status).HasMaxLength(50);
            });
        }
    }
}
