using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace AgricultureApp.MauiClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<PlatformInfoService>();
            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddTransient<JwtAuthHandler>();

            builder.Services.AddHttpClient("ApiClient",
                client =>
                {
                    client.BaseAddress = new Uri(Constants.ApiBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    HttpClientHandler handler = new();
#if DEBUG
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        return cert != null && cert.Issuer.Equals("CN=localhost")
                            ? true
                            : errors == System.Net.Security.SslPolicyErrors.None;
                    };
#endif
                    return handler;
                })
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddSingleton<IFarmHubClient>(sp =>
            {
                AuthenticationService authService = sp.GetRequiredService<AuthenticationService>();
                ILogger<FarmHubClient> logger = sp.GetRequiredService<ILogger<FarmHubClient>>();
                return new FarmHubClient(authService, logger);
            });

            builder.Services.AddScoped<FarmRepository>();
            builder.Services.AddScoped<UserRepository>();

            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddSingleton<ProfilePageModel>();

            builder.Services.AddTransientWithShellRoute<VerifyTwoFactorPage, VerifyTwoFactorPageModel>(nameof(VerifyTwoFactorPage));
            builder.Services.AddTransientWithShellRoute<FarmDetailPage, FarmDetailPageModel>("farm");

            return builder.Build();
        }
    }
}
