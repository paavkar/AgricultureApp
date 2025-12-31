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
                .AddHttpMessageHandler<JwtAuthHandler>();

            builder.Services.AddSingleton<FarmRepository>();

            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<LoginPageModel>();
            builder.Services.AddSingleton<ProfilePageModel>();
            builder.Services.AddSingleton<VerifyTwoFactorPageModel>();

            Routing.RegisterRoute(nameof(VerifyTwoFactorPage), typeof(VerifyTwoFactorPage));

            return builder.Build();
        }
    }
}
