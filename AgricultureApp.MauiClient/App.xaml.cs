using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AgricultureApp.MauiClient
{
    public partial class App : Application
    {
        public readonly AuthenticationService AuthService;
        private readonly ILogger<App> _logger;

        public static new App Current => (App)Application.Current;

        public App(
            AuthenticationService auth,
            ILogger<App> logger)
        {
            InitializeComponent();
            AuthService = auth;
            _logger = logger;
        }

        protected override async void OnResume()
        {
            base.OnResume();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new()
            {
                Page = new LoadingPage(),
                Title = "AgricultureApp"
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
                if (preferredCulture.Equals("en-GB", StringComparison.OrdinalIgnoreCase)
                    || preferredCulture.Equals("fi-FI", StringComparison.OrdinalIgnoreCase))
                {
                    await SecureStorage.SetAsync("preferred_culture", preferredCulture);
                }
                else
                {
                    preferredCulture = "en-GB";
                }
            }

            ApplyCulture(preferredCulture);

            if (isAuthenticated)
            {
                var result = await AuthService.RefreshTokensAsync();

                if (result)
                {
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
                    window.Page = new AppShell();
                }
                else
                {
#if WINDOWS || MACCATALYST
                    window.Width = 600;
                    window.Height = 750;
                    window.MinimumWidth = 600;
                    window.MinimumHeight = 750;
                    window.MaximumWidth = 600;
                    window.MaximumHeight = 750;
#endif
                    window.Page = new AuthShell();
                }
            }
            else
            {
#if WINDOWS || MACCATALYST
                window.Width = 600;
                window.Height = 750;
                window.MinimumWidth = 600;
                window.MinimumHeight = 750;
                window.MaximumWidth = 600;
                window.MaximumHeight = 750;
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