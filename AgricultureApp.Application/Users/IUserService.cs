using AgricultureApp.Application.ResultModels;

namespace AgricultureApp.Application.Users
{
    public interface IUserService
    {
        Task<UserResult> GetByIdAsync(string userId);
    }
}
