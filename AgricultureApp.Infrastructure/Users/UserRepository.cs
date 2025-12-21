using AgricultureApp.Application.Users;
using AgricultureApp.Domain.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.Infrastructure.Users
{
    public class UserRepository(
        IConfiguration configuration,
        ILogger<UserRepository> logger) : IUserRepository
    {
        private SqlConnection GetConnection()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            const string sql = """
                SELECT * FROM AspNetUsers WHERE Id = @UserId
                """;
            using SqlConnection connection = GetConnection();
            try
            {
                ApplicationUser user = await connection.QueryFirstAsync<ApplicationUser>(sql, new { UserId = userId });

                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user with ID {UserId}", userId);
                return null;
            }
        }
    }
}
