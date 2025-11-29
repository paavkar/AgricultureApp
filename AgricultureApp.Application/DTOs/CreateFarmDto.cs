using AgricultureApp.Domain.Farms;

namespace AgricultureApp.Application.DTOs
{
    public class CreateFarmDto
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public Farm ToFarmModel(CreateFarmDto dto, string userId)
        {
            return new Farm
            {
                Id = Guid.CreateVersion7().ToString(),
                Name = dto.Name,
                Location = dto.Location,
                OwnerId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null,
                CreatedBy = userId,
                UpdatedBy = null
            };
        }
    }
}
