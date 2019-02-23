using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
  public  class ForgotPasswordViewModel:INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private readonly NavigationService _navigationService;
        private readonly DataService _dataService;
        private bool _isRunning;
        private bool _isEnabled;
        #endregion

        #region Properties
        public string Email { get; set; }

        public bool IsRunning
        {
            set {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get {
                return _isRunning;
            }
        }

        public bool IsEnabled
        {
            set {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get {
                return _isEnabled;
            }
        }
        #endregion

        #region Constructor
        public ForgotPasswordViewModel()
        {
            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService = new DataService();

            IsEnabled = true;
        }
        #endregion

        #region Commands
        public ICommand SendNewPasswordCommand { get { return new RelayCommand(SendNewPassword); } }

        private async void SendNewPassword()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("notasti.com");
            if (!isReachable)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            if (string.IsNullOrEmpty(Email))
            {
                await _dialogService.ShowMessage("Error", "You must enter your email.");
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            var parameters = _dataService.First<Parameter>(false);
            var response = await _apiService.PasswordRecovery(
                parameters.UrlBase, "/api", "/Users/PasswordRecovery", Email);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                await _dialogService.ShowMessage("Error", "Your password can't be recovered.");
                return;
            }

            await _dialogService.ShowMessage("Confirmation", "Your new password has been sent, check the new password in your email.");
            _navigationService.SetMainPage("LoginPage");
        }

        public ICommand CancelCommand { get { return new RelayCommand(Cancel); } }

        private void Cancel()
        {
            _navigationService.SetMainPage("LoginPage");
        }
        #endregion
    }

}
