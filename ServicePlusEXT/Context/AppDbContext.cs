

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
            modelBuilder.Entity<ApplicationAttribute>()
                .HasOne(a => a.ServiceApplication)
                .WithMany(s => s.Attributes)
                .HasForeignKey(a => a.ServiceApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
