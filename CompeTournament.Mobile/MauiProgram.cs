using CommunityToolkit.Maui;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Mobile.Core.ViewModels;
using CompeTournament.Mobile.Services;
using CompeTournament.Mobile.Views;
using Microsoft.AspNetCore.SignalR.Client;
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

            var apiClient = builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(ApiConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
#if DEBUG
            apiClient.ConfigurePrimaryHttpMessageHandler(DevelopmentHttpsHandler.Create);
#endif

            builder.Services.AddTransient<ILiveTournamentClient>(sp =>
            {
                var tokenStore = sp.GetRequiredService<ITokenStore>();
                var connection = new HubConnectionBuilder()
                    .WithUrl($"{ApiConstants.BaseUrl}hubs/tournament", options =>
                    {
                        options.AccessTokenProvider = () => tokenStore.GetTokenAsync();
#if DEBUG
                        options.HttpMessageHandlerFactory = _ => DevelopmentHttpsHandler.Create();
                        options.WebSocketConfiguration = socket => socket.RemoteCertificateValidationCallback = DevelopmentHttpsHandler.ValidateCertificate;
#endif
                    })
                    .WithAutomaticReconnect()
                    .Build();
                return new LiveTournamentClient(connection);
            });

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<GroupsViewModel>();
            builder.Services.AddTransient<CreateGroupViewModel>();
            builder.Services.AddTransient<GroupDetailViewModel>();
            builder.Services.AddTransient<GroupManageViewModel>();
            builder.Services.AddTransient<MatchPredictionViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<GroupsPage>();
            builder.Services.AddTransient<CreateGroupPage>();
            builder.Services.AddTransient<GroupDetailPage>();
            builder.Services.AddTransient<GroupManagePage>();
            builder.Services.AddTransient<MatchPredictionPage>();
            builder.Services.AddTransient<ProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
