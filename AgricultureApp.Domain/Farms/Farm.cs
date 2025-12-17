using AgricultureApp.Domain.Users;

namespace AgricultureApp.Domain.Farms
{
    public class Farm : FarmBase
    {
        // Navigation Properties
        public ICollection<FarmManager> Managers { get; set; } = [];
        public ApplicationUser Owner { get; set; }
        public ICollection<Field> Fields { get; set; } = [];
        public ICollection<Field> OwnedFields { get; set; } = [];

        public FarmDto ToDto()
        {
            return new FarmDto
            {
                Id = this.Id,
                Name = this.Name,
                Location = this.Location,
                OwnerId = this.OwnerId,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
                CreatedBy = this.CreatedBy,
                UpdatedBy = this.UpdatedBy,
                Managers = []
            };
        }
    }
}
