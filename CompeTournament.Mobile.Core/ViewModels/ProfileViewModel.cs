namespace CompeTournament.Mobile.Core.ViewModels
{
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
