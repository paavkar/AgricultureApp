using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Farms
{
    public class FarmService(
        ILogger<FarmService> logger,
        IFarmRepository farmRepository) : IFarmService
    {
        public async Task<FarmResult> CreateAsync(CreateFarmDto farmDto, string userId)
        {
            Farm farm = farmDto.ToFarmModel(farmDto, userId);
            var rownsAffected = await farmRepository.AddAsync(farm);

            if (rownsAffected == 0)
            {
                logger.LogError("Failed to create farm for owner {OwnerId}", userId);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = ["Failed to create farm."]
                };
            }

            logger.LogInformation("Successfully created farm {FarmName} - {FarmId} for owner {OwnerId}",
                farm.Name, farm.Id, farm.OwnerId);
            return new FarmResult
            {
                Succeeded = true,
                Farm = farm
            };
        }
        public Task<FarmResult> GetByIdAsync(string farmId) => throw new NotImplementedException();
        public Task<FarmListResult> GetByOwnerAsync(string ownerId) => throw new NotImplementedException();

        public async Task<FarmResult> UpdateAsync(UpdateFarmDto farmDto, string userId)
        {
            Farm? existingFarm = await farmRepository.GetByIdAsync(farmDto.Id);
            if (existingFarm == null)
            {
                logger.LogWarning("Farm with ID {FarmId} not found for update.", farmDto.Id);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = ["Farm not found."]
                };
            }

            var rowsAffected = await farmRepository.UpdateAsync(farmDto, userId);
            if (rowsAffected == 0)
            {
                logger.LogError("Failed to update farm {FarmId}.", farmDto.Id);
                return new FarmResult
                {
                    Succeeded = false,
                    Errors = ["Failed to update farm."]
                };
            }

            logger.LogInformation("Successfully updated farm {FarmId}. Update by {UserId}.", farmDto.Id, userId);

            return new FarmResult
            {
                Succeeded = true,
                UpdatedFarm = farmDto
            };
        }
    }
}
