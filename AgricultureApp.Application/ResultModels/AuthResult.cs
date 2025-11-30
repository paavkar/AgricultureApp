namespace AgricultureApp.Application.ResultModels
{
    public class AuthResult : BaseResult
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
