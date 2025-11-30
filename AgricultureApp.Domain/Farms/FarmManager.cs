using AgricultureApp.Domain.Users;

namespace AgricultureApp.Domain.Farms
{
    public class FarmManager
    {
        public string FarmId { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset AssignedAt { get; set; }

        // Navigation Properties
        public Farm Farm { get; set; }
        public ApplicationUser Manager { get; set; }
    }
}
