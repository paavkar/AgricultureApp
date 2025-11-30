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
        public async Task<FarmResult<Farm>> CreateAsync(CreateFarmDto farmDto, string userId)
        {
            Farm farm = farmDto.ToFarmModel(farmDto, userId);
            var rownsAffected = await farmRepository.AddAsync(farm);

            if (rownsAffected == 0)
            {
                logger.LogError("Failed to create farm for owner {OwnerId}", userId);
                return new FarmResult<Farm>
                {
                    Succeeded = false,
                    Errors = ["Failed to create farm."]
                };
            }

            logger.LogInformation("Successfully created farm {FarmName} - {FarmId} for owner {OwnerId}",
                farm.Name, farm.Id, farm.OwnerId);
            return new FarmResult<Farm>
            {
                Succeeded = true,
                Farm = farm
            };
        }
        public Task<FarmResult<Farm>> GetByIdAsync(string farmId) => throw new NotImplementedException();

        public async Task<FarmResult<FarmDto>> GetFullInfoAsync(string farmId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(farmId);

            return farm is null
                ? new FarmResult<FarmDto>
                {
                    Succeeded = false,
                    Errors = ["Farm not found."]
                }
                : new FarmResult<FarmDto>
                {
                    Succeeded = true,
                    Farm = farm
                };
        }
        public Task<FarmListResult> GetByOwnerAsync(string ownerId) => throw new NotImplementedException();

        public async Task<FarmResult<Farm>> UpdateAsync(UpdateFarmDto farmDto, string userId)
        {
            var rowsAffected = await farmRepository.UpdateAsync(farmDto, userId);
            if (rowsAffected == 0)
            {
                logger.LogError("Failed to update farm {FarmId}.", farmDto.Id);
                return new FarmResult<Farm>
                {
                    Succeeded = false,
                    Errors = ["Failed to update farm."]
                };
            }

            logger.LogInformation("Successfully updated farm {FarmId}. Update by {UserId}.", farmDto.Id, userId);

            return new FarmResult<Farm>
            {
                Succeeded = true,
                UpdatedFarm = farmDto
            };
        }

        public async Task<BaseResult> DeleteAsync(string farmId, string userId)
        {
            var rowsAffected = await farmRepository.DeleteAsync(farmId, userId);
            if (rowsAffected == 0)
            {
                logger.LogError("Failed to delete farm {FarmId}.", farmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Failed to delete farm."]
                };
            }

            logger.LogInformation("Successfully deleted farm {FarmId}.", farmId);

            return new BaseResult
            {
                Succeeded = true
            };
        }
    }
}
