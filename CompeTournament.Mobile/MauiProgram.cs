using CommunityToolkit.Maui;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Mobile.Core.ViewModels;
using CompeTournament.Mobile.Services;
using CompeTournament.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace CompeTournament.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit();

            builder.Services.AddSingleton<ITokenStore, SecureStorageTokenStore>();
            builder.Services.AddSingleton<ISession, Session>();
            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

            builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(ApiConstants.BaseUrl);
            });

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<GroupsViewModel>();
            builder.Services.AddTransient<GroupDetailViewModel>();
            builder.Services.AddTransient<MatchPredictionViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<GroupsPage>();
            builder.Services.AddTransient<GroupDetailPage>();
            builder.Services.AddTransient<MatchPredictionPage>();
            builder.Services.AddTransient<ProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
