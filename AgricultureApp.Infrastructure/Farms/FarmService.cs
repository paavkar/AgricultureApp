using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;
using AgricultureApp.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Farms
{
    public class FarmService(
        ILogger<FarmService> logger,
        IFarmRepository farmRepository,
        UserManager<ApplicationUser> userManager) : IFarmService
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

        public async Task<ManagerResult> AddManagerAsync(string userId, string farmId, string email)
        {
            Farm? farm = await farmRepository.GetByIdAsync(farmId);

            if (farm is null)
            {
                return new ManagerResult
                {
                    Succeeded = false,
                    Errors = ["Farm not found."]
                };
            }

            if (farm.OwnerId != userId)
            {
                return new ManagerResult
                {
                    Succeeded = false,
                    Errors = ["Only the farm owner can add managers."]
                };
            }

            ApplicationUser? user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return new ManagerResult
                {
                    Succeeded = false,
                    Errors = ["User not found with given email."]
                };
            }

            if (farm.OwnerId == user.Id)
            {
                return new ManagerResult
                {
                    Succeeded = false,
                    Errors = ["The owner cannot be added as a manager."]
                };
            }

            DateTimeOffset assigned = DateTimeOffset.UtcNow;
            var rowsAffected = await farmRepository.AddManagerAsync(farmId, user.Id, assigned);

            return rowsAffected == 0
                ? new ManagerResult
                {
                    Succeeded = false,
                    Errors = ["Failed to add manager to farm."]
                }
                : new ManagerResult
                {
                    Succeeded = true,
                    FarmManager = new FarmManagerDto
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email!,
                        AssignedAt = assigned
                    }
                };
        }
    }
}
