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
    }
}
