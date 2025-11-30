using AgricultureApp.Domain.Farms;
using Microsoft.AspNetCore.Identity;

namespace AgricultureApp.Domain.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }

        public ICollection<Farm> OwnedFarms { get; set; } = [];
        public ICollection<FarmManager> ManagedFarms { get; set; } = [];
    }
}
