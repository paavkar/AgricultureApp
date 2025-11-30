using AgricultureApp.Domain.Farms;
using AgricultureApp.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgricultureApp.Infrastructure.Persistence
{
    public class AgricultureAppDbContext(DbContextOptions<AgricultureAppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Farm> Farms { get; set; }
        public DbSet<FarmManager> FarmManagers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Farm>()
                .HasOne(f => f.Owner)
                .WithMany(u => u.OwnedFarms)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FarmManager>()
                .HasKey(fm => new { fm.FarmId, fm.UserId });

            builder.Entity<FarmManager>()
                .HasOne(fm => fm.Farm)
                .WithMany(f => f.Managers)
                .HasForeignKey(fm => fm.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FarmManager>()
                .HasOne(fm => fm.Manager)
                .WithMany(u => u.ManagedFarms)
                .HasForeignKey(fm => fm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
