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
        public Task<Farm?> GetByIdAsync(string farmId) => throw new NotImplementedException();
        public Task<IEnumerable<Farm>?> GetByOwnerAsync(string ownerId) => throw new NotImplementedException();
    }
}
