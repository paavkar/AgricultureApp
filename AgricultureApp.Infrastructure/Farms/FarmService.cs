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
                    Errors = [localizer["FailedToCreateFarm"]]
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

        public async Task<FarmResult<FarmDto>> GetFullInfoAsync(string farmId)
        {
            FarmDto? farm = await farmRepository.GetFullInfoAsync(farmId);

            return farm is null
                ? new FarmResult<FarmDto>
                {
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["FarmNotFound"]]
                }
                : new FarmResult<FarmDto>
                {
                    Succeeded = true,
                    Farm = farm
                };
        }

        public async Task<FarmResult<FarmDto>> GetByOwnerAsync(string ownerId)
        {
            IEnumerable<FarmDto>? farms = await farmRepository.GetByOwnerAsync(ownerId);

            return farms is null
                ? new FarmResult<FarmDto>
                {
                    Succeeded = false,
                    Errors = [localizer["DatabaseErrorFarmsFetch"]]
                }
                : new FarmResult<FarmDto>
                {
                    Succeeded = true,
                    Farms = farms
                };
        }

        public async Task<FarmResult<FarmDto>> GetByManagerAsync(string managerId)
        {
            IEnumerable<FarmDto>? farms = await farmRepository.GetByManagerAsync(managerId);

            return farms is null
                ? new FarmResult<FarmDto>
                {
                    Succeeded = false,
                    Errors = [localizer["DatabaseErrorFarmsFetch"]]
                }
                : new FarmResult<FarmDto>
                {
                    Succeeded = true,
                    Farms = farms
                };
        }

        public async Task<FarmResult<Farm>> UpdateAsync(UpdateFarmDto farmDto, string userId)
        {
            var rowsAffected = await farmRepository.UpdateAsync(farmDto, userId);

            if (rowsAffected == 0)
            {
                logger.LogError("Failed to update farm {FarmId}.", farmDto.Id);
                return new FarmResult<Farm>
                {
                    Succeeded = false,
                    Errors = [localizer["UpdateFarmFailed"]]
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
                    Errors = [localizer["DeleteFarmFailed"]]
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
            var isOwner = await farmRepository.IsUserOwnerAsync(farmId, userId);

            if (!isOwner)
            {
                return new ManagerResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["OnlyFarmOwnerPermittedManagers"]]
                };
            }

            ApplicationUser? user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return new ManagerResult
                {
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["UserNotFoundEmail"]]
                };
            }

            if (user.Id == userId)
            {
                return new ManagerResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["FarmOwnerNotManager"]]
                };
            }

            DateTimeOffset assigned = DateTimeOffset.UtcNow;
            var rowsAffected = await farmRepository.AddManagerAsync(farmId, user.Id, assigned);

            if (rowsAffected == 0)
            {
                return new ManagerResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["AddManagerFailed"]]
                };
            }

            await notificationService.NotifyUserAddedToFarmAsync(user.Id, farmId);
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
            var isOwner = await farmRepository.IsUserOwnerAsync(farmId, userId);

            if (!isOwner)
            {
                return new BaseResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["OnlyFarmOwnerPermittedManagers"]]
                };
            }

            var result = await farmRepository.DeleteManagerAsync(farmId, managerId);

            if (result == 0)
            {
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["DeleteManagerFailed"]]
                };
            }

            await notificationService.NotifyUserRemovedFromFarmAsync(managerId, farmId);
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["OwnerFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                logger.LogError("Failed to create field. User {UserId} is not authorized to add fields to owner farm {OwnerFarmId}.",
                    userId, fieldDto.OwnerFarmId);
                return new FieldResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["UserNotAuthorizedField"]]
                };
            }

            if (farm.OwnedFields.Any(f => f.Name.Equals(fieldDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                logger.LogError("Failed to create field. Field with name {FieldName} already exists in farm {OwnerFarmId}.",
                    fieldDto.Name, fieldDto.OwnerFarmId);
                return new FieldResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["FieldAlreadyExists"]]
                };
            }

            Field field = fieldDto.ToFieldModel();
            var rowsAffected = await farmRepository.AddFieldAsync(field);

            if (rowsAffected == 0)
            {
                logger.LogError("Failed to create field for farm {FarmId}", field.FarmId);
                return new FieldResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["CreateFieldFailed"]]
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["FieldNotFound"]]
                };
            }

            var isOwner = await farmRepository.IsUserOwnerAsync(field.OwnerFarm.Id, userId);
            var isManager = await farmRepository.IsUserFarmManagerAsync(field.OwnerFarm.Id, userId);

            if (!isOwner && !isManager)
            {
                var isCurrentOwner = await farmRepository.IsUserOwnerAsync(field.CurrentFarm.Id, userId);
                var isCurrentManager = await farmRepository.IsUserFarmManagerAsync(field.CurrentFarm.Id, userId);

                if (!isCurrentOwner && !isCurrentManager)
                {
                    logger.LogError("User {UserId} is not authorized to access field {FieldId}.",
                        userId, fieldId);
                    return new FieldResult
                    {
                        StatusCode = 403,
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
            var isOwner = await farmRepository.IsUserOwnerAsync(update.OwnerFarmId, userId);
            var isManager = await farmRepository.IsUserFarmManagerAsync(update.OwnerFarmId, userId);

            if (!isOwner && !isManager)
            {
                logger.LogError("User {UserId} is not authorized to update fields in owner farm {OwnerFarmId}.",
                    userId, update.OwnerFarmId);
                return new BaseResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["UserNotAuthorizedField"]]
                };
            }

            var updated = await farmRepository.UpdateFieldCurrentFarmAsync(fieldId, update.FarmId);

            if (!updated)
            {
                logger.LogError("Failed to update current farm for field {FieldId} to farm {FarmId}.", fieldId, update.FarmId);
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["UpdateFieldFarm"]]
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
            var isOwner = await farmRepository.IsUserOwnerAsync(update.OwnerFarmId, userId);
            var isManager = await farmRepository.IsUserFarmManagerAsync(update.OwnerFarmId, userId);

            if (!isOwner && !isManager)
            {
                var isCurrentOwner = await farmRepository.IsUserOwnerAsync(update.FarmId, userId);
                var isCurrentManager = await farmRepository.IsUserFarmManagerAsync(update.FarmId, userId);

                if (!isCurrentOwner && !isCurrentManager)
                {
                    logger.LogError("User {UserId} is not authorized to revert field management {FieldId}.",
                    userId, fieldId);
                    return new BaseResult
                    {
                        StatusCode = 403,
                        Succeeded = false,
                        Errors = [localizer["UserNotAuthorizedField"]]
                    };
                }
            }

            var updated = await farmRepository.RevertFieldCurrentFarmAsync(fieldId);

            if (!updated)
            {
                logger.LogError("Failed to revert the field's {FieldId} current farm to owner farm.", fieldId);
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["UpdateFieldFarm"]]
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["OwnerFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                logger.LogError("User {UserId} is not authorized to update fields in owner farm {OwnerFarmId}.",
                    userId, fieldDto.OwnerFarmId);
                return new BaseResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["UserNotAuthorizedField"]]
                };
            }

            if (fieldDto.Size < 0)
            {
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["FieldSizeValidation"]]
                };
            }

            if (farm.OwnedFields.Any(f => f.Name.Equals(fieldDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["UniqueFieldValidation"]]
                };
            }

            var updated = await farmRepository.UpdateFieldAsync(fieldDto);

            if (!updated)
            {
                logger.LogError("Failed to update field {FieldId}.", fieldDto.FieldId);
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["UpdateFieldFailed"]]
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
            var isOwner = await farmRepository.IsUserOwnerAsync(update.FarmId, userId);
            var isManager = await farmRepository.IsUserFarmManagerAsync(update.FarmId, userId);

            if (!isOwner && !isManager)
            {
                logger.LogError("User {UserId} is not authorized to update fields in farm {FarmId}.",
                    userId, update.FarmId);
                return new BaseResult
                {
                    StatusCode = 403,
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
                    StatusCode = 400,
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new FieldCultivationResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != cultivationDto.FieldId))
            {
                return new FieldCultivationResult
                {
                    StatusCode = 404,
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
                    StatusCode = 400,
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
            var isOwner = await farmRepository.IsUserOwnerAsync(farmId, userId);
            var isManager = await farmRepository.IsUserFarmManagerAsync(farmId, userId);

            if (!isOwner && !isManager)
            {
                return new FieldCultivationResult
                {
                    StatusCode = 403,
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new BaseResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != harvestDto.FieldId))
            {
                return new BaseResult
                {
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            FieldCultivationDto? cultivation = await farmRepository.GetFieldCultivationByIdAsync(harvestDto.FieldCultivationId);

            if (cultivation is null)
            {
                return new BaseResult
                {
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultivationNotFound"]]
                };
            }

            if (cultivation.PlantingDate >= harvestDto.HarvestDate)
            {
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["HarvestDateBeforePlantingDate"]]
                };
            }

            if (harvestDto.ActualYield < 0)
            {
                return new BaseResult
                {
                    StatusCode = 400,
                    Succeeded = false,
                    Errors = [localizer["ActualYieldNegative"]]
                };
            }

            var result = await farmRepository.UpdateFieldHarvestedAsync(harvestDto);

            if (!result)
            {
                return new BaseResult
                {
                    StatusCode = 400,
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new BaseResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != update.FieldId))
            {
                return new BaseResult
                {
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            var result = await farmRepository.UpdateFieldCultivationStatusAsync(update.FieldCultivationId, update.Status);

            if (!result)
            {
                return new BaseResult
                {
                    StatusCode = 400,
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
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultFarmNotFound"]]
                };
            }

            if (farm.OwnerId != userId && !farm.Managers.Any(m => m.UserId == userId))
            {
                return new BaseResult
                {
                    StatusCode = 403,
                    Succeeded = false,
                    Errors = [localizer["CultivatingFarmNotAuthorizedFC"]]
                };
            }

            if (farm.Fields.All(f => f.Id != deleteItems.FieldId))
            {
                return new BaseResult
                {
                    StatusCode = 404,
                    Succeeded = false,
                    Errors = [localizer["CultivatedFieldNotFound"]]
                };
            }

            var result = await farmRepository.DeleteFieldCultivationAsync(deleteItems.FieldCultivationId);

            if (!result)
            {
                return new BaseResult
                {
                    StatusCode = 400,
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
