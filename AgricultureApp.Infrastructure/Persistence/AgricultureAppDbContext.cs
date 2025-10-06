using Microsoft.EntityFrameworkCore;

namespace AgricultureApp.Infrastructure.Persistence
{
    public class AgricultureAppDbContext : DbContext
    {
        public AgricultureAppDbContext(DbContextOptions<AgricultureAppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
