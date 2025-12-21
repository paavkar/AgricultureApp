using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.LLM;
using AgricultureApp.Domain.Farms;
using AgricultureApp.Domain.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Farms
{
    public class FarmRepository(
        ILogger<FarmRepository> logger,
        IConfiguration configuration) : IFarmRepository
    {
        private SqlConnection GetConnection()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        }

        public async Task<int> AddAsync(Farm farm)
        {
            const string sql = """
                INSERT INTO Farms (Id, Name, Location, OwnerId, CreatedAt, CreatedBy)
                VALUES (@Id, @Name, @Location, @OwnerId, @CreatedAt, @CreatedBy)
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql, farm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting farm: {Method}", nameof(AddAsync));
                return 0;
            }
        }

        public async Task<Farm?> GetByIdAsync(string farmId)
        {
            const string sql = """
                SELECT *
                FROM Farms
                WHERE Id = @Id
                """;
            using SqlConnection connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<Farm>(sql, new { Id = farmId });
        }

        public async Task<FarmDto?> GetFullInfoAsync(string farmId)
        {
            const string sql = """
                SELECT * FROM Farms f WHERE f.Id = @Id;
                SELECT * FROM FarmManagers fm WHERE fm.FarmId = @Id;
                SELECT u.* FROM AspNetUsers u
                INNER JOIN Farms f ON u.Id = f.OwnerId
                WHERE f.Id = @Id;
                SELECT * FROM AspNetUsers u
                INNER JOIN FarmManagers fm ON u.Id = fm.UserId
                WHERE fm.FarmId = @Id;
                SELECT * FROM Fields fld WHERE fld.FarmId = @Id;
                SELECT * FROM Fields fld WHERE fld.OwnerFarmId = @Id;
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(sql, new { Id = farmId });
                Farm? farm = await multi.ReadFirstOrDefaultAsync<Farm>();

                if (farm is null)
                    return null;
                FarmDto farmDto = farm.ToDto();

                IEnumerable<FarmManager> managers = await multi.ReadAsync<FarmManager>();
                ApplicationUser? owner = await multi.ReadFirstOrDefaultAsync<ApplicationUser>();

                if (owner is null)
                    return null;

                IEnumerable<ApplicationUser> managerUsers = await multi.ReadAsync<ApplicationUser>();

                farmDto.Owner = new FarmPerson
                {
                    UserId = owner?.Id!,
                    Name = owner?.Name!,
                    Email = owner?.Email!
                };

                farmDto.Managers = [.. managers.Select(m =>
                {
                    ApplicationUser? user = managerUsers.FirstOrDefault(u => u.Id == m.UserId);
                    return new FarmManagerDto
                    {
                        UserId = m.UserId,
                        Name = user?.Name!,
                        Email = user?.Email!,
                        AssignedAt = m.AssignedAt
                    };
                })];

                IEnumerable<FieldDto> fields = await multi.ReadAsync<FieldDto>();
                IEnumerable<FieldDto> ownedFields = await multi.ReadAsync<FieldDto>();

                farmDto.Fields = [.. fields];
                farmDto.OwnedFields = [.. ownedFields];

                return farmDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving full farm info: {Method}", nameof(GetFullInfoAsync));
                return null;
            }
        }

        public async Task<IEnumerable<FarmDto>?> GetByOwnerAsync(string ownerId)
        {
            const string sql = """
                SELECT f.*, fm.*, u.*, mu.*, fd.*, ofd.*
                FROM Farms f
                LEFT JOIN FarmManagers fm ON f.Id = fm.FarmId
                LEFT JOIN AspNetUsers u ON f.OwnerId = u.Id
                LEFT JOIN AspNetUsers mu ON fm.UserId = mu.Id
                LEFT JOIN Fields fd ON f.Id = fd.FarmId
                LEFT JOIN Fields ofd ON f.Id = ofd.OwnerFarmId
                WHERE f.OwnerId = @OwnerId
                """;
            return await GetFarmsAsync(sql, new { OwnerId = ownerId }, nameof(GetByOwnerAsync));
        }

        public async Task<IEnumerable<FarmDto>?> GetByManagerAsync(string managerId)
        {
            const string sql = """
                SELECT f.*, fm.*, u.*, mu.*, fd.*, ofd.*
                FROM Farms f
                LEFT JOIN FarmManagers fm ON f.Id = fm.FarmId
                LEFT JOIN AspNetUsers u ON f.OwnerId = u.Id
                LEFT JOIN AspNetUsers mu ON fm.UserId = mu.Id
                LEFT JOIN Fields fd ON f.Id = fd.FarmId
                LEFT JOIN Fields ofd ON f.Id = ofd.OwnerFarmId
                WHERE fm.UserId = @ManagerId
                """;
            return await GetFarmsAsync(sql, new { ManagerId = managerId }, nameof(GetByManagerAsync));
        }

        public async Task<IEnumerable<FarmDto>?> GetFarmsAsync(string sql, object args, string methodName)
        {
            Dictionary<string, FarmDto> farmDictionary = [];
            using SqlConnection connection = GetConnection();
            try
            {
                IEnumerable<FarmDto> farms = await connection.QueryAsync<FarmDto, FarmManager, ApplicationUser, ApplicationUser, FieldDto, FieldDto, FarmDto>(
                    sql,
                    (farm, manager, owner, managerUser, cultivatedField, ownedField) =>
                    {
                        if (!farmDictionary.TryGetValue(farm.Id, out FarmDto? currentFarm))
                        {
                            currentFarm = farm;
                            currentFarm.Managers = [];
                            currentFarm.Fields = [];
                            currentFarm.OwnedFields = [];
                            currentFarm.Owner = new FarmPerson
                            {
                                UserId = owner.Id,
                                Name = owner.Name,
                                Email = owner.Email
                            };
                            farmDictionary.Add(currentFarm.Id, currentFarm);
                        }

                        if (manager != null && managerUser != null && !currentFarm.Managers.Any(m => m.UserId == manager.UserId))
                        {
                            currentFarm.Managers.Add(new FarmManagerDto
                            {
                                UserId = manager.UserId,
                                Name = managerUser.Name,
                                Email = managerUser.Email,
                                AssignedAt = manager.AssignedAt
                            });
                        }

                        if (cultivatedField != null && !currentFarm.Fields.Any(f => f.Id == cultivatedField.Id))
                        {
                            currentFarm.Fields.Add(cultivatedField);
                        }

                        if (ownedField != null && !currentFarm.OwnedFields.Any(f => f.Id == ownedField.Id))
                        {
                            currentFarm.OwnedFields.Add(ownedField);
                        }

                        return currentFarm;
                    },
                    args,
                    splitOn: "FarmId,Id,Id,Id,Id"
                );

                return farms.Distinct();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving farms: {Method}", methodName);
                return [];
            }
        }

        public async Task<int> UpdateAsync(UpdateFarmDto farmDto, string userId)
        {
            const string sql = """
                UPDATE Farms
                SET Name = @Name,
                    OwnerId = @OwnerId,
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy
                WHERE Id = @Id
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql, new
                {
                    farmDto.Name,
                    farmDto.OwnerId,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedBy = userId,
                    farmDto.Id
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating farm: {Method}", nameof(UpdateAsync));
                return 0;
            }
        }

        public async Task<int> DeleteAsync(string farmId, string userId)
        {
            const string sql = """
                DELETE FROM Farms
                WHERE Id = @Id
                AND OwnerId = @OwnerId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql, new { Id = farmId, OwnerId = userId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting farm: {Method}", nameof(DeleteAsync));
                return 0;
            }
        }

        public async Task<int> AddManagerAsync(string farmId, string userId, DateTimeOffset assigned)
        {
            const string sql = """
                INSERT INTO FarmManagers (FarmId, UserId, AssignedAt)
                VALUES (@FarmId, @UserId, @AssignedAt)
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql,
                    new
                    {
                        FarmId = farmId,
                        UserId = userId,
                        AssignedAt = assigned
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding farm manager: {Method}", nameof(AddManagerAsync));
                return 0;
            }
        }

        public async Task<int> DeleteManagerAsync(string farmId, string userId)
        {
            const string sql = """
                DELETE FROM FarmManagers
                WHERE FarmId = @FarmId
                AND UserId = @UserId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql,
                    new
                    {
                        FarmId = farmId,
                        UserId = userId
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting farm manager: {Method}", nameof(DeleteManagerAsync));
                return 0;
            }
        }

        public async Task<bool> IsUserFarmManagerAsync(string farmId, string userId)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM FarmManagers
                WHERE FarmId = @FarmId
                AND UserId = @UserId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var count = await connection.ExecuteScalarAsync<int>(sql,
                    new
                    {
                        FarmId = farmId,
                        UserId = userId
                    });
                return count > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking farm manager: {Method}", nameof(IsUserFarmManagerAsync));
                return false;
            }
        }

        public async Task<int> AddFieldAsync(Field field)
        {
            const string sql = """
                INSERT INTO Fields (Id, Name, Size, SizeUnit, Status, SoilType, FarmId, OwnerFarmId)
                VALUES (@Id, @Name, @Size, @SizeUnit, @Status, @SoilType, @FarmId, @OwnerFarmId)
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql, field);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting field: {Method}", nameof(AddFieldAsync));
                return 0;
            }
        }

        public async Task<FieldDto?> GetFieldByIdAsync(string fieldId)
        {
            const string sql = """
                SELECT f.*, cf.*, owf.*, fc.*, fm.*
                FROM Fields f
                LEFT JOIN Farms cf ON f.FarmId = cf.Id
                LEFT JOIN Farms owf ON f.OwnerFarmId = owf.Id
                LEFT JOIN FieldCultivations fc ON f.Id = fc.FieldId
                LEFT JOIN Farms fm ON fc.FarmId = fm.Id
                WHERE f.Id = @FieldId
                """;

            Dictionary<string, FieldDto> fieldDictionary = [];
            using SqlConnection connection = GetConnection();
            try
            {
                await connection.QueryAsync<FieldDto, FarmDto, FarmDto, FieldCultivationDto, FarmDto, FieldDto>(
                    sql,
                    (field, cultivatingFarm, ownerFarm, fieldCultivation, cultivatedFarm) =>
                    {
                        if (!fieldDictionary.TryGetValue(field.Id, out FieldDto? currentField))
                        {
                            currentField = field;
                            currentField.CurrentFarm = cultivatingFarm;
                            currentField.OwnerFarm = ownerFarm;
                            currentField.Cultivations = [];
                            fieldDictionary.Add(currentField.Id, currentField);
                        }

                        if (fieldCultivation != null)
                        {
                            fieldCultivation.CultivatedFarm = cultivatedFarm;
                            currentField.Cultivations.Add(fieldCultivation);
                        }

                        return currentField;
                    },
                    new { FieldId = fieldId },
                    splitOn: "Id,Id,Id,Id"
                );

                return fieldDictionary.FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving field info: {Method}", nameof(GetFieldByIdAsync));
                return null;
            }
        }

        public async Task<LlmField?> GetFieldByNameAsync(string fieldName, string farmId)
        {
            const string sql = """
                SELECT f.*, cf.*, owf.*, fc.*, fm.*
                FROM Fields f
                LEFT JOIN Farms cf ON f.FarmId = cf.Id
                LEFT JOIN Farms owf ON f.OwnerFarmId = owf.Id
                LEFT JOIN FieldCultivations fc ON f.Id = fc.FieldId
                LEFT JOIN Farms fm ON fc.FarmId = fm.Id
                WHERE cf.Id = @FarmId
                AND f.Name = @FieldName
                """;
            Dictionary<string, LlmField> fieldDictionary = [];
            using SqlConnection connection = GetConnection();
            try
            {
                await connection.QueryAsync<FieldDto, FarmDto, FarmDto, FieldCultivationDto, FarmDto, LlmField>(
                    sql,
                    (field, cultivatingFarm, ownerFarm, fieldCultivation, cultivatedFarm) =>
                    {
                        if (!fieldDictionary.TryGetValue(field.Id, out LlmField? currentField))
                        {
                            currentField = LlmField.FromDto(field);
                            currentField.Status = field.Status.ToString();
                            currentField.SoilType = field.SoilType.ToString();
                            currentField.CurrentFarm = cultivatingFarm;
                            currentField.OwnerFarm = ownerFarm;
                            currentField.Cultivations = [];
                            fieldDictionary.Add(currentField.Id, currentField);
                        }

                        if (fieldCultivation != null)
                        {
                            fieldCultivation.CultivatedFarm = cultivatedFarm;
                            LlmCultivation cult = LlmCultivation.FromDto(fieldCultivation);
                            cult.Status = fieldCultivation.Status.ToString();
                            currentField.Cultivations.Add(cult);
                        }

                        return currentField;
                    },
                    new { FarmId = farmId, FieldName = fieldName },
                    splitOn: "Id,Id,Id,Id"
                );

                return fieldDictionary.FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving field info: {Method}", nameof(GetFieldByIdAsync));
                return null;
            }
        }

        public async Task<bool> CheckFieldExists(string fieldName, string farmId)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM Fields
                WHERE Name = @Name
                AND FarmId = @FarmId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var count = await connection.ExecuteScalarAsync<int>(sql, new { Name = fieldName, FarmId = farmId });
                return count > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking field existence: {Method}", nameof(CheckFieldExists));
                return false;
            }
        }

        public async Task<bool> UpdateFieldCurrentFarmAsync(string fieldId, string farmId)
        {
            const string sql = """
                UPDATE Fields
                SET FarmId = @FarmId
                WHERE Id = @FieldId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { FarmId = farmId, FieldId = fieldId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating field current farm: {Method}", nameof(UpdateFieldCurrentFarmAsync));
                return false;
            }
        }

        public async Task<bool> RevertFieldCurrentFarmAsync(string fieldId)
        {
            const string sql = """
                UPDATE Fields
                SET FarmId = OwnerFarmId
                WHERE Id = @FieldId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { FieldId = fieldId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating field current farm: {Method}", nameof(UpdateFieldCurrentFarmAsync));
                return false;
            }
        }

        public async Task<bool> UpdateFieldAsync(UpdateFieldDto fieldDto)
        {
            const string sql = """
                UPDATE Fields
                SET Name = @Name,
                    Size = @Size,
                    SizeUnit = @SizeUnit,
                    Status = @Status,
                    SoilType = @SoilType
                WHERE Id = @FieldId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, fieldDto);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating field info: {Method}", nameof(UpdateFieldAsync));
                return false;
            }
        }

        public async Task<bool> UpdateFieldStatusAsync(string fieldId, FieldStatus status)
        {
            const string sql = """
                UPDATE Fields
                SET Status = @Status
                WHERE Id = @FieldId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { Status = status, FieldId = fieldId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating field status: {Method}", nameof(UpdateFieldStatusAsync));
                return false;
            }
        }

        public async Task<int> AddFieldCultivationAsync(FieldCultivation cultivation)
        {
            const string sql = """
                INSERT INTO FieldCultivations (Id, Crop, ExpectedYield, YieldUnit, Status, PlantingDate, FieldId, FarmId)
                VALUES (@Id, @Crop, @ExpectedYield, @YieldUnit, @Status, @PlantingDate, @FieldId, @FarmId)
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                return await connection.ExecuteAsync(sql, cultivation);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting field cultivation: {Method}", nameof(AddFieldCultivationAsync));
                return 0;
            }
        }

        public async Task<IEnumerable<FieldCultivationDto>> GetFieldCultivationsAsync(string fieldId)
        {
            const string sql = """
                SELECT fc.*, f.*, fm.*
                FROM FieldCultivations fc
                INNER JOIN Fields f ON f.Id = fc.FieldId
                INNER JOIN Farms fm ON fm.Id = fc.FarmId
                WHERE fc.FieldId = @FieldId;
                """;

            using SqlConnection connection = GetConnection();
            try
            {
                IEnumerable<FieldCultivationDto> cultivations = await connection.QueryAsync<FieldCultivationDto, FieldDto, FarmDto, FieldCultivationDto>(
                    sql,
                    (cultivation, field, farm) =>
                    {
                        cultivation.Field = field;
                        cultivation.CultivatedFarm = farm;
                        return cultivation;
                    },
                    new { FieldId = fieldId }
                );

                return cultivations;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving full farm info: {Method}", nameof(GetFullInfoAsync));
                return null;
            }
        }

        public async Task<FieldCultivationDto?> GetFieldCultivationByIdAsync(string cultivationId)
        {
            const string sql = """
                SELECT * FROM FieldCultivations
                WHERE Id = @Id
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                FieldCultivationDto? cultivation = await connection.QueryFirstOrDefaultAsync<FieldCultivationDto>(sql, new { Id = cultivationId });

                return cultivation;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving field cultivation by ID: {Method}", nameof(GetFieldCultivationByIdAsync));
                return null;
            }
        }

        public async Task<bool> UpdateFieldHarvestedAsync(FieldHarvestDto harvestDto)
        {
            const string sql = """
                UPDATE FieldCultivations
                SET ActualYield = @ActualYield,
                    YieldUnit = @YieldUnit,
                    Status = @Status,
                    HarvestDate = @HarvestDate
                WHERE Id = @FieldCultivationId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, harvestDto);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating field harvested info: {Method}", nameof(UpdateFieldHarvestedAsync));
                return false;
            }
        }

        public async Task<bool> UpdateFieldCultivationStatusAsync(string cultivationId, CultivationStatus status)
        {
            const string sql = """
                UPDATE FieldCultivations
                SET Status = @Status
                WHERE Id = @CultivationId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    Status = status,
                    CultivationId = cultivationId
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating field cultivation status: {Method}", nameof(UpdateFieldCultivationStatusAsync));
                return false;
            }
        }

        public async Task<bool> DeleteFieldCultivationAsync(string cultivationId)
        {
            const string sql = """
                DELETE FROM FieldCultivations
                WHERE Id = @CultivationId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { CultivationId = cultivationId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting field cultivation: {Method}", nameof(DeleteFieldCultivationAsync));
                return false;
            }
        }
    }
}
