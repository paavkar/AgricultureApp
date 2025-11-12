using AgricultureApp.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgricultureApp.Infrastructure.Persistence
{
    public class AgricultureAppDbContext(DbContextOptions<AgricultureAppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder) => base.OnModelCreating(builder);
    }
}
