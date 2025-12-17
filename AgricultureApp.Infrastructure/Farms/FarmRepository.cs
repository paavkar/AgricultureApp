using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
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
                farmDto.Managers = managers.Select(m =>
                {
                    ApplicationUser? user = managerUsers.FirstOrDefault(u => u.Id == m.UserId);
                    return new FarmManagerDto
                    {
                        UserId = m.UserId,
                        Name = user?.Name!,
                        Email = user?.Email!,
                        AssignedAt = m.AssignedAt
                    };
                });

                IEnumerable<Field> fields = await multi.ReadAsync<Field>();
                IEnumerable<Field> ownedFields = await multi.ReadAsync<Field>();

                farmDto.Fields = fields;
                farmDto.OwnedFields = ownedFields;

                return farmDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving full farm info: {Method}", nameof(GetFullInfoAsync));
                return null;
            }
        }

        public Task<IEnumerable<Farm>?> GetByOwnerAsync(string ownerId) => throw new NotImplementedException();

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
    }
}
