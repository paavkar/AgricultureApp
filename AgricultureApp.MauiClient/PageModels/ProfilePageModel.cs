using AgricultureApp.MauiClient.Resources.Strings;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class ProfilePageModel : ObservableObject
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        private readonly ILogger<ProfilePageModel> _logger;
        private readonly ModalErrorHandler _errorHandler;
        private readonly UserRepository _userRepository;
        private readonly AuthenticationService _auth;

        [ObservableProperty]
        private ApplicationUser _currentUser;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        public bool _is2FASetupVisible;

        [ObservableProperty]
        public bool _is2FARemoveVisible;

        [ObservableProperty]
        private bool _isIn2FASetup;

        [ObservableProperty]
        private ImageSource _qrCodeImage;

        [ObservableProperty]
        private string _twoFactorCode;

        public ProfilePageModel(
            ILogger<ProfilePageModel> logger,
            ModalErrorHandler errorHandler,
            UserRepository userRepository,
            AuthenticationService auth)
        {
            _logger = logger;
            _errorHandler = errorHandler;
            _userRepository = userRepository;
            _auth = auth;
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                UserResult result = await _userRepository.GetLoggedInUserAsync();

                if (result.Succeeded)
                {
                    CurrentUser = result.User;
                    Is2FASetupVisible = !CurrentUser.TwoFactorEnabled;
                    Is2FARemoveVisible = CurrentUser.TwoFactorEnabled;
                }
                else
                {
                    await AppShell.Current.GoToAsync("//MainPage");
                    return;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
            _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;

        [RelayCommand]
        private async Task Appearing()
        {
            if (!_dataLoaded)
            {
                _dataLoaded = true;
                await Refresh();
            }
            // This means we are being navigated to
            else if (!_isNavigatedTo)
            {
                await Refresh();
            }
        }

        [RelayCommand]
        private async Task Setup2FA()
        {
            AuthResult result = await _auth.SetupTwoFactorAsync();

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    await AppShell.Current.DisplaySnackbar(error);
                }
                return;
            }

            var otpAuthUrl = result.TwoFactorUri;
            _logger.LogInformation(otpAuthUrl);

            QrCodeImage = GenerateQrCode(otpAuthUrl);

            IsIn2FASetup = true;
        }

        private ImageSource GenerateQrCode(string otpAuthUrl)
        {
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpAuthUrl, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);

            return ImageSource.FromStream(() => new MemoryStream(qrBytes));
        }

        [RelayCommand]
        private async Task Confirm2FA()
        {
            if (string.IsNullOrWhiteSpace(TwoFactorCode))
            {
                await AppShell.Current.DisplaySnackbar("");
            }

            VerifyTwoFactorDto dto = new()
            {
                Code = TwoFactorCode
            };

            AuthResult result = await _auth.EnableTwoFactorAsync(dto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    await AppShell.Current.DisplaySnackbar(error);
                    return;
                }
            }

            CurrentUser.TwoFactorEnabled = true;

            Is2FASetupVisible = false;
            Is2FARemoveVisible = true;
            IsIn2FASetup = false;

            await AppShell.Current.DisplaySnackbar(AppResources.TwoFactorEnabled);
        }

        [RelayCommand]
        private async Task Remove2FA()
        {
            var confirm = await Shell.Current.DisplayAlertAsync(
                "Remove 2FA",
                "Are you sure you want to disable two‑factor authentication?",
                "Yes", "No");

            if (!confirm)
                return;
            AuthResult result = await _auth.DisableTwoFactorAsync();

            if (result.Succeeded)
            {
                CurrentUser.TwoFactorEnabled = false;
                Is2FASetupVisible = true;
                Is2FARemoveVisible = false;
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    await AppShell.Current.DisplaySnackbar(error);
                }
            }
            return;
        }
    }
}
