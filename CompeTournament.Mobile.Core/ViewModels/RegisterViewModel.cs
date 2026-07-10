namespace CompeTournament.Mobile.Core.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CompeTournament.Mobile.Core.Services;
    using CompeTournament.Shared.Auth;

    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigation;

        public RegisterViewModel(IApiClient apiClient, INavigationService navigation)
        {
            _apiClient = apiClient;
            _navigation = navigation;
        }

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirm = string.Empty;

        [ObservableProperty]
        private string? _successMessage;

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy)
            {
                return;
            }

            ClearError();
            SuccessMessage = null;

            if (Password != Confirm)
            {
                ShowError("Las contrasenas no coinciden.");
                return;
            }

            try
            {
                IsBusy = true;
                await _apiClient.RegisterAsync(new RegisterRequest
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    Password = Password,
                    Confirm = Confirm
                });

                SuccessMessage = "Cuenta creada. Ya puedes iniciar sesion.";
                await _navigation.GoBackAsync();
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
        private Task CancelAsync() => _navigation.GoBackAsync();
    }
}
