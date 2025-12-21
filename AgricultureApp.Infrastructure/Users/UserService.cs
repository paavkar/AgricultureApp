using AgricultureApp.Application.ResultModels;
using AgricultureApp.Application.Users;
using AgricultureApp.Domain.Users;
using AgricultureApp.SharedKernel.Localization;
using Microsoft.Extensions.Localization;

namespace AgricultureApp.Infrastructure.Users
{
    public class UserService(
        IUserRepository userRepository,
        IStringLocalizer<AgricultureAppLoc> localizer) : IUserService
    {
        public async Task<UserResult> GetByIdAsync(string userId)
        {
            ApplicationUser? user = await userRepository.GetByIdAsync(userId);

            return user is not null
                ? new UserResult { Succeeded = true, User = user }
                : new UserResult { Succeeded = false, Errors = [localizer["UserNotFound"]], StatusCode = 404 };
        }
    }
}
