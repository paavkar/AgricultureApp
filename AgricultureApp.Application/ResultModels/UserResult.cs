using AgricultureApp.Domain.Users;

namespace AgricultureApp.Application.ResultModels
{
    public class UserResult : BaseResult
    {
        public ApplicationUser User { get; set; }
    }
}
