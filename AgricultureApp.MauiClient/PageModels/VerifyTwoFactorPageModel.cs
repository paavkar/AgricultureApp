using AgricultureApp.MauiClient.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class VerifyTwoFactorPageModel : ObservableObject, IQueryAttributable
    {
        private readonly AuthenticationService _auth;
        private readonly ILogger<VerifyTwoFactorPageModel> _logger;

        [ObservableProperty]
        private string _twoFactorCode;

        public LoginDto LoginData { get; set; }

        public VerifyTwoFactorPageModel(AuthenticationService auth, ILogger<VerifyTwoFactorPageModel> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("LoginData", out var value))
            {
                LoginData = value as LoginDto;
            }
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
            else
            {
                foreach (var error in result.Errors)
                {
                    await AuthShell.Current.DisplayAlertAsync(AppResources.Error, error, "Ok");
                }
            }
        }
    }
}
