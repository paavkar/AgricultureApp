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
        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldCultivation> FieldCultivations { get; set; }

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

            builder.Entity<Field>()
                .HasOne(f => f.CurrentFarm)
                .WithMany(farm => farm.Fields)
                .HasForeignKey(f => f.FarmId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Field>()
                .HasOne(f => f.OwnerFarm)
                .WithMany(farm => farm.OwnedFields)
                .HasForeignKey(f => f.OwnerFarmId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FieldCultivation>()
                .HasOne(fc => fc.Field)
                .WithMany(f => f.Cultivations)
                .HasForeignKey(fc => fc.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FieldCultivation>()
                .HasOne(fc => fc.CultivatedFarm)
                .WithMany()
                .HasForeignKey(fc => fc.FarmId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
