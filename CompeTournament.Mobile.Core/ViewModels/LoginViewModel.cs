namespace CompeTournament.Mobile.Core.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Auth;

    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ITokenStore _tokenStore;
        private readonly ISession _session;
        private readonly INavigationService _navigation;

        public LoginViewModel(IApiClient apiClient, ITokenStore tokenStore, ISession session, INavigationService navigation)
        {
            _apiClient = apiClient;
            _tokenStore = tokenStore;
            _session = session;
            _navigation = navigation;
        }

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Ingresa tu correo y contrasena.");
                return;
            }

            try
            {
                IsBusy = true;
                var response = await _apiClient.LoginAsync(new TokenRequest
                {
                    Username = Email.Trim(),
                    Password = Password
                });

                await _tokenStore.SetTokenAsync(response.Token);
                _session.SetUser(response.User);
                Password = string.Empty;

                await _navigation.GoToAsync($"//{AppRoutes.Groups}");
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
        private Task GoToRegisterAsync() => _navigation.GoToAsync(AppRoutes.Register);
    }
}
