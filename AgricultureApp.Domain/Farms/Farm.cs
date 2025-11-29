using AgricultureApp.Domain.Users;

namespace AgricultureApp.Domain.Farms
{
    public class Farm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string OwnerId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public ICollection<FarmManager> Managers { get; set; } = [];
        public ApplicationUser Owner { get; set; }
    }
}
