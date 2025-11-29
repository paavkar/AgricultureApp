using AgricultureApp.Application.DTOs;
using AgricultureApp.Application.Farms;
using AgricultureApp.Domain.Farms;
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
