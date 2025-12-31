using AgricultureApp.MauiClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgricultureApp.MauiClient.PageModels
{
    [QueryProperty(nameof(LoginDto), "LoginData")]
    public partial class VerifyTwoFactorPageModel : ObservableObject
    {
        private readonly AuthenticationService _auth;

        [ObservableProperty]
        private string _twoFactorCode;

        public LoginDto LoginData { get; set; }

        public VerifyTwoFactorPageModel(AuthenticationService auth)
        {
            _auth = auth;
        }

        [RelayCommand]
        private async Task VerifyTwoFactor()
        {
            if (string.IsNullOrWhiteSpace(TwoFactorCode))
            {
                await AuthShell.DisplaySnackbarAsync("Please enter the two-factor authentication code.");
                return;
            }

            TwoFactorDto dto = new()
            {
                Email = LoginData.Email,
                UserName = LoginData.UserName,
                Code = TwoFactorCode
            };

            AuthResult result = await _auth.VerifyTwoFactorAsync(dto);

            if (result.Succeeded)
            {
                Window window = Application.Current!.Windows[0];
                window.Page = new AppShell();
            }
        }
    }
}
