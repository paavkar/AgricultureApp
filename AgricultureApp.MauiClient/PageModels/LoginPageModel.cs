using AgricultureApp.MauiClient.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        private readonly AuthenticationService _auth;
        private readonly ILogger<LoginPageModel> _logger;

        [ObservableProperty]
        private string _email;
        [ObservableProperty]
        private string _userName;
        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _useEmail = true;
        public string LoginModeButtonText =>
            UseEmail ? AppResources.UserNameLogin : AppResources.EmailLogin;

        public LoginPageModel(AuthenticationService auth, ILogger<LoginPageModel> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [RelayCommand]
        private void ToggleLoginMode()
        {
            UseEmail = !UseEmail;
        }

        [RelayCommand]
        private async Task Login()
        {
            if ((string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(UserName))
                || string.IsNullOrWhiteSpace(Password))
            {
                await AuthShell.DisplaySnackbarAsync("Email or UserName and Password cannot be empty.");
                return;
            }
            LoginDto dto = new() { Email = Email, UserName = UserName, Password = Password };

            AuthResult result = await _auth.LoginAsync(dto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    await AuthShell.DisplaySnackbarAsync(error);
                }
                return;
            }
            Window window = Application.Current!.Windows[0];

            if (result.TwoFactorRequired)
            {
                await Shell.Current.GoToAsync(nameof(VerifyTwoFactorPage),
                    new Dictionary<string, object>
                    {
                        { "LoginData", dto }
                    });
            }
            else
            {
                window.Page = new AppShell();
#if WINDOWS
                var displayInfo = DeviceDisplay.MainDisplayInfo;
                var density = displayInfo.Density;

                var width = displayInfo.Width / density;
                var height = displayInfo.Height / density;

                window.Width = width * 0.6;
                window.Height = height * 0.6;
                window.MinimumWidth = width * 0.5;
                window.MinimumHeight = height * 0.5;
#endif
            }
        }
    }
}
