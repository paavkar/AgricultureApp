using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AgricultureApp.MauiClient
{
    public partial class App : Application
    {
        private readonly AuthenticationService _auth;
        private readonly ILogger<App> _logger;
        public App(AuthenticationService auth, ILogger<App> logger)
        {
            InitializeComponent();
            _auth = auth;
            _logger = logger;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new()
            {
                Page = new LoadingPage()
            };

            _ = InitializeRootPage(window);

            return window;
        }

        private async Task InitializeRootPage(Window window)
        {
            var accessToken = await SecureStorage.GetAsync("access_token");
            var refreshToken = await SecureStorage.GetAsync("refresh_token");
            var preferredCulture = await SecureStorage.GetAsync("preferred_culture");

            var isAuthenticated =
                !string.IsNullOrWhiteSpace(accessToken) &&
                !string.IsNullOrWhiteSpace(refreshToken);

            if (string.IsNullOrWhiteSpace(preferredCulture))
            {
                preferredCulture = CultureInfo.CurrentCulture.Name;
                await SecureStorage.SetAsync("preferred_culture", preferredCulture);
            }

            ApplyCulture(preferredCulture);

            if (isAuthenticated)
            {
                var result = await _auth.RefreshTokensAsync();

                if (result)
                {
                    window.Page = new AppShell();
                }
                else
                {
#if WINDOWS
                    window.Width = 600;
                    window.Height = 750;
                    window.MinimumWidth = 600;
                    window.MinimumHeight = 750;
#endif
                    window.Page = new AuthShell();
                }
            }
            else
            {
#if WINDOWS
                window.Width = 600;
                window.Height = 750;
                window.MinimumWidth = 600;
                window.MinimumHeight = 750;
#endif
                window.Page = new AuthShell();
            }
        }

        public static void ApplyCulture(string culture)
        {
            CultureInfo ci = new(culture);

            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}