namespace CompeTournament.Mobile.Core.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Auth;
    using CompeTournament.Shared.Tournaments;

    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ITokenStore _tokenStore;
        private readonly ISession _session;
        private readonly INavigationService _navigation;

        public ProfileViewModel(IApiClient apiClient, ITokenStore tokenStore, ISession session, INavigationService navigation)
        {
            _apiClient = apiClient;
            _tokenStore = tokenStore;
            _session = session;
            _navigation = navigation;
        }

        public UserDto? CurrentUser => _session.CurrentUser;

        [ObservableProperty]
        private InsightsDto? _insights;

        public ObservableCollection<Badge> Badges { get; } = new();

        partial void OnInsightsChanged(InsightsDto? value)
        {
            Badges.Clear();
            foreach (var badge in BuildBadges(value))
            {
                Badges.Add(badge);
            }
        }

        public static IReadOnlyList<Badge> BuildBadges(InsightsDto? insights)
        {
            var badges = new List<Badge>();
            if (insights == null)
            {
                return badges;
            }

            if (insights.ExactHits + insights.OutcomeHits >= 1)
            {
                badges.Add(new Badge { Name = "Primer acierto", Description = "Acertaste tu primera predicción." });
            }

            if (insights.ExactHits >= 5)
            {
                badges.Add(new Badge { Name = "Francotirador", Description = "5 marcadores exactos." });
            }

            if (insights.CurrentStreak >= 3)
            {
                badges.Add(new Badge { Name = "En racha", Description = "3 aciertos seguidos." });
            }

            if (insights.BestStreak >= 5)
            {
                badges.Add(new Badge { Name = "Imparable", Description = "Mejor racha de 5 o más." });
            }

            if (insights.TotalPoints >= 100)
            {
                badges.Add(new Badge { Name = "Centenario", Description = "100 puntos acumulados." });
            }

            if (insights.TotalResolved >= 10 && insights.Accuracy >= 0.7)
            {
                badges.Add(new Badge { Name = "Experto", Description = "70% de acierto con 10 o más predicciones." });
            }

            return badges;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();

            try
            {
                IsBusy = true;
                var user = await _apiClient.GetMeAsync();
                _session.SetUser(user);
                OnPropertyChanged(nameof(CurrentUser));
                Insights = await _apiClient.GetInsightsAsync();
            }
            catch (Exception ex)
            {
                ShowError(DescribeError(ex));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                try
                {
                    await _apiClient.LogoutAsync(refreshToken);
                }
                catch (Exception)
                {
                }
            }

            await _tokenStore.ClearAsync();
            _session.Clear();
            await _navigation.GoToAsync($"//{AppRoutes.Login}");
        }
    }
}
