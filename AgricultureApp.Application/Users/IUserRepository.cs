using AgricultureApp.Domain.Users;

namespace AgricultureApp.Application.Users
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string userId);
    }
}
