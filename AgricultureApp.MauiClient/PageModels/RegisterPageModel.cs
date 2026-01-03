using AgricultureApp.MauiClient.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class RegisterPageModel(
        AuthenticationService auth,
        ILogger<RegisterPageModel> logger) : ObservableObject
    {
        [ObservableProperty]
        private string _email;
        [ObservableProperty]
        private string _userName;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _confirmPassword;

        [RelayCommand]
        private async Task Register()
        {
            var errorCount = 0;
            var errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Email))
            {
                errorCount++;
                errorMessage += $"{AppResources.EmailRequired}\n";
            }
            if (string.IsNullOrWhiteSpace(UserName))
            {
                errorCount++;
                errorMessage += $"{AppResources.UserNameRequired}\n";
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                errorCount++;
                errorMessage += $"{AppResources.NameRequired}\n";
            }
            if (!string.Equals(Password, ConfirmPassword))
            {
                errorCount++;
                errorMessage += $"{AppResources.ConfirmPasswordMustMatch}\n";
            }
            if (errorCount > 0)
            {
                await AuthShell.DisplaySnackbarAsync(errorMessage);
                return;
            }
            RegisterDto dto = new() { Email = Email, UserName = UserName, Name = Name, Password = Password };

            AuthResult result = await auth.RegisterAsync(dto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    errorMessage += $"{error}\n";
                }
                await AuthShell.DisplaySnackbarAsync(errorMessage);
                return;
            }
            Window window = Application.Current!.Windows[0];
            window.Page = new AppShell();
#if WINDOWS || MACCATALYST
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
