using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.Notifications;
using AgricultureApp.Application.ResultModels;
using AgricultureApp.Domain.Farms;
using AgricultureApp.Domain.Users;
using AgricultureApp.SharedKernel.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Farms
{
    public class FarmService(
        ILogger<FarmService> logger,
        IFarmRepository farmRepository,
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<AgricultureAppLoc> localizer,
        IFarmNotificationService notificationService) : IFarmService
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
        public Task<FarmResult<FarmDto>> GetByOwnerAsync(string ownerId) => throw new NotImplementedException();

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
                    Errors = ["Only the farm owner can manage managers."]
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

            if (rowsAffected == 0)
            {
                return new ManagerResult
                {
                    Succeeded = false,
                    Errors = ["Failed to add manager to farm."]
                };
            }

            await notificationService.NotifyUserAddedToFarmAsync(userId, farmId);
            return new ManagerResult
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

        public async Task<BaseResult> DeleteManagerAsync(string farmId, string userId, string managerId)
        {
            Farm? farm = await farmRepository.GetByIdAsync(farmId);

            if (farm is null)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Farm not found."]
                };
            }

            if (farm.OwnerId != userId)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Only the farm owner can manage managers."]
                };
            }

            var result = await farmRepository.DeleteManagerAsync(farmId, managerId);

            if (result == 0)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Failed to remove manager with given ID from farm."]
                };
            }

            await notificationService.NotifyUserRemovedFromFarmAsync(userId, farmId);
            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<FieldResult> CreateFieldAsync(CreateFieldDto fieldDto, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(fieldDto.OwnerFarmId);

            if (farm is null)
            {
                logger.LogError("Failed to create field. Owner farm {OwnerFarmId} not found.", fieldDto.OwnerFarmId);
                return new FieldResult
                {
                    Succeeded = false,
                    Errors = ["Owner farm not found."]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                logger.LogError("Failed to create field. User {UserId} is not authorized to add fields to owner farm {OwnerFarmId}.",
                    userId, fieldDto.OwnerFarmId);
                return new FieldResult
                {
                    Succeeded = false,
                    Errors = ["The user is not authorized to add fields to the farm."]
                };
            }

            if (farm.OwnedFields.Any(f => f.Name.Equals(fieldDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                logger.LogError("Failed to create field. Field with name {FieldName} already exists in farm {OwnerFarmId}.",
                    fieldDto.Name, fieldDto.OwnerFarmId);
                return new FieldResult
                {
                    Succeeded = false,
                    Errors = ["Field with the same name already exists in the owner farm."]
                };
            }

            Field field = fieldDto.ToFieldModel();
            var rowsAffected = await farmRepository.AddFieldAsync(field);

            if (rowsAffected == 0)
            {
                logger.LogError("Failed to create field for farm {FarmId}", field.FarmId);
                return new FieldResult
                {
                    Succeeded = false,
                    Errors = ["Failed to create field."]
                };
            }
            logger.LogInformation("Successfully created field {FieldName} - {FieldId} for farm {FarmId}",
                field.Name, field.Id, field.FarmId);

            FieldDto dto = field.ToDto();

            dto.CurrentFarm = farm;
            dto.OwnerFarm = farm;

            await notificationService.NotifyFieldAddedAsync(farm.Id, dto);

            return new FieldResult
            {
                Succeeded = true,
                Field = dto
            };
        }

        public async Task<FieldResult> GetFieldByIdAsync(string fieldId, string userId)
        {
            FieldDto? field = await farmRepository.GetFieldByIdAsync(fieldId);

            if (field is null)
            {
                logger.LogError("Field {FieldId} not found.", fieldId);
                return new FieldResult
                {
                    Succeeded = false,
                    Errors = [localizer["FieldNotFound"]]
                };
            }

            FarmDto? ownerFarm = await farmRepository.GetFullInfoAsync(field.OwnerFarm.Id);

            if (ownerFarm is null)
            {
                logger.LogError("Owner farm {OwnerFarmId} for field {FieldId} not found.", field.OwnerFarm.Id, fieldId);
                return new FieldResult
                {
                    Succeeded = false,
                    Errors = [localizer["OwnerFarmNotFound"]]
                };
            }

            if (ownerFarm.OwnerId != userId && !ownerFarm.Managers.Any(m => m.UserId == userId))
            {
                FarmDto? currentFarm = await farmRepository.GetFullInfoAsync(field.CurrentFarm.Id);

                if (currentFarm is null)
                {
                    logger.LogError("Current farm {CurrentFarmId} for field {FieldId} not found.", field.CurrentFarm.Id, fieldId);
                    return new FieldResult
                    {
                        Succeeded = false,
                        Errors = [localizer["CultFarmNotFound"]]
                    };
                }

                if (currentFarm.OwnerId != userId && !currentFarm.Managers.Any(m => m.UserId == userId))
                {
                    logger.LogError("User {UserId} is not authorized to access field {FieldId}.",
                        userId, fieldId);
                    return new FieldResult
                    {
                        Succeeded = false,
                        Errors = [localizer["UserNotAuthorizedField"]]
                    };
                }
            }

            return new FieldResult
            {
                Succeeded = true,
                Field = field
            };
        }

        public async Task<BaseResult> UpdateFieldCurrentFarmAsync(
            string fieldId, UpdateFieldFarmDto update, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(update.OwnerFarmId);

            if (farm is null)
            {
                logger.LogError("Owner farm {OwnerFarmId} not found.", update.OwnerFarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Owner farm not found."]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                logger.LogError("User {UserId} is not authorized to update fields in owner farm {OwnerFarmId}.",
                    userId, update.OwnerFarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["The user is not authorized to update fields in the owner farm."]
                };
            }

            var updated = await farmRepository.UpdateFieldCurrentFarmAsync(fieldId, update.FarmId);

            if (!updated)
            {
                logger.LogError("Failed to update current farm for field {FieldId} to farm {FarmId}.", fieldId, update.FarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Failed to update field's current farm."]
                };
            }

            await notificationService.NotifyFieldCultChangeAsync(update.OwnerFarmId, update);
            await notificationService.NotifyFieldCultChangeAsync(update.FarmId, update);
            logger.LogInformation("Successfully updated current farm for field {FieldId} to farm {FarmId}.", fieldId, update.FarmId);

            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<BaseResult> RevertFieldCurrentFarmAsync(
            string fieldId, UpdateFieldFarmDto update, string userId)
        {
            FarmDto? ownerFarm = await farmRepository.GetFullInfoAsync(update.OwnerFarmId);

            if (ownerFarm is null)
            {
                logger.LogError("Owner farm {OwnerFarmId} not found.", update.OwnerFarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Owner farm not found."]
                };
            }

            if (ownerFarm.OwnerId != userId && !ownerFarm.Managers.Any(m => m.UserId == userId))
            {
                FarmDto? farm = await farmRepository.GetFullInfoAsync(update.FarmId);

                if (farm is null)
                {
                    logger.LogError("Managing farm {FarmId} not found.", update.FarmId);
                    return new BaseResult
                    {
                        Succeeded = false,
                        Errors = ["Managing farm not found."]
                    };
                }

                if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
                {
                    logger.LogError("User {UserId} is not authorized to revert field management {FieldId}.",
                    userId, fieldId);
                    return new BaseResult
                    {
                        Succeeded = false,
                        Errors = ["The user is not authorized to revert managing relationship of given field."]
                    };
                }
            }

            var updated = await farmRepository.RevertFieldCurrentFarmAsync(fieldId);

            if (!updated)
            {
                logger.LogError("Failed to revert the field's {FieldId} current farm to owner farm.", fieldId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Failed to update field's current farm."]
                };
            }

            await notificationService.NotifyFieldCultChangeAsync(update.OwnerFarmId, update);
            await notificationService.NotifyFieldCultChangeAsync(update.FarmId, update);
            logger.LogInformation("Successfully reverted the field's {FieldId} current farm to owner farm.", fieldId);

            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<BaseResult> UpdateFieldAsync(UpdateFieldDto fieldDto, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(fieldDto.OwnerFarmId);

            if (farm is null)
            {
                logger.LogError("Owner farm {OwnerFarmId} not found.", fieldDto.OwnerFarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Owner farm not found."]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                logger.LogError("User {UserId} is not authorized to update fields in owner farm {OwnerFarmId}.",
                    userId, fieldDto.OwnerFarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["The user is not authorized to update fields in the owner farm."]
                };
            }

            if (fieldDto.Size < 0)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Size must be more than 0."]
                };
            }

            if (farm.OwnedFields.Any(f => f.Name.Equals(fieldDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Field must have a unique name within a farm."]
                };
            }

            var updated = await farmRepository.UpdateFieldAsync(fieldDto);

            if (!updated)
            {
                logger.LogError("Failed to update field {FieldId}.", fieldDto.FieldId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = ["Failed to update field."]
                };
            }

            await notificationService.NotifyFieldUpdatedAsync(farm.Id, fieldDto);
            logger.LogInformation("Successfully updated field {FieldId}. Update by {UserId}.", fieldDto.FieldId, userId);

            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<BaseResult> UpdateFieldStatusAsync(UpdateFieldStatusDto update, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(update.FarmId);

            if (farm is null)
            {
                logger.LogError("Farm {FarmId} not found.", update.FarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                logger.LogError("User {UserId} is not authorized to update fields in farm {FarmId}.",
                    userId, update.FarmId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedF"]]
                };
            }

            var result = await farmRepository.UpdateFieldStatusAsync(update.FieldId, update.Status);

            if (!result)
            {
                logger.LogError("Failed to update status for field {FieldId}.", update.FieldId);
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["FieldStatusUpdateFailed"]]
                };
            }

            await notificationService.NotifyFieldStatusChangedAsync(update.FarmId, update);
            logger.LogInformation("Successfully updated status for field {FieldId} to {Status}.", update.FieldId, update.Status);
            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<FieldCultivationResult> AddFieldCultivationAsync(
            CreateFieldCultivationDto cultivationDto, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(cultivationDto.FarmId);

            if (farm is null)
            {
                return new FieldCultivationResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new FieldCultivationResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != cultivationDto.FieldId))
            {
                return new FieldCultivationResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            FieldCultivation fieldCultivation = cultivationDto.ToFieldCultivationModel();
            var rowsAffected = await farmRepository.AddFieldCultivationAsync(fieldCultivation);

            if (rowsAffected == 0)
            {
                return new FieldCultivationResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivationFailed"]]
                };
            }

            await notificationService.NotifyFieldCultivationAddedAsync(farm.Id, fieldCultivation.ToDto());
            return new FieldCultivationResult
            {
                Succeeded = true,
                FieldCultivation = fieldCultivation.ToDto()
            };
        }

        public async Task<FieldCultivationResult> GetFieldCultivationsAsync(string fieldId, string farmId, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(farmId);

            if (farm is null)
            {
                return new FieldCultivationResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new FieldCultivationResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorized"]]
                };
            }

            IEnumerable<FieldCultivationDto> cultivations = await farmRepository.GetFieldCultivationsAsync(fieldId);

            return new FieldCultivationResult
            {
                Succeeded = true,
                FieldCultivations = cultivations
            };
        }

        public async Task<BaseResult> SetFieldHarvestedAsync(FieldHarvestDto harvestDto, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(harvestDto.FarmId);

            if (farm is null)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != harvestDto.FieldId))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            FieldCultivationDto? cultivation = await farmRepository.GetFieldCultivationByIdAsync(harvestDto.FieldCultivationId);

            if (cultivation is null)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivationNotFound"]]
                };
            }

            if (cultivation.PlantingDate >= harvestDto.HarvestDate)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["HarvestDateBeforePlantingDate"]]
                };
            }

            if (harvestDto.ActualYield < 0)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["ActualYieldNegative"]]
                };
            }

            var result = await farmRepository.UpdateFieldHarvestedAsync(harvestDto);

            if (!result)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["FieldHarvestUpdateFailed"]]
                };
            }

            await notificationService.NotifyFieldHarvestedAsync(harvestDto.FarmId, harvestDto);
            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<BaseResult> UpdateFieldCultivationStatusAsync(UpdateFieldCultivationStatusDto update, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(update.FarmId);

            if (farm is null)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != update.FieldId))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            var result = await farmRepository.UpdateFieldCultivationStatusAsync(update.FieldCultivationId, update.Status);

            if (!result)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["FieldCultivationStatusUpdateFailed"]]
                };
            }

            await notificationService.NotifyFieldCultivationStatusUpdatedAsync(update.FarmId, update);
            return new BaseResult
            {
                Succeeded = true
            };
        }

        public async Task<BaseResult> DeleteFieldCultivationAsync(DeleteFieldCultivationDto deleteItems, string userId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(deleteItems.FarmId);

            if (farm is null)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != deleteItems.FieldId))
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            var result = await farmRepository.DeleteFieldCultivationAsync(deleteItems.FieldCultivationId);

            if (!result)
            {
                return new BaseResult
                {
                    Succeeded = false,
                    Errors = [localizer["FieldCultivationDeleteFailed"]]
                };
            }

            await notificationService.NotifyFieldCultivationDeletedAsync(deleteItems.FarmId, deleteItems.FieldCultivationId);
            return new BaseResult
            {
                Succeeded = true
            };
        }
    }
}
