using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Classes;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
  public  class ChangePasswordViewModel:INotifyPropertyChanged
    {
        #region Eventos
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructores

        public ChangePasswordViewModel()
        {
            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService = new DataService();

          IsEnabled = true;
        }


        #endregion

        #region Attributes
        private ApiService apiService;
        private DialogService dialogService;
        private DataService dataService;
        private NavigationService navigationService;
       private bool isRunning;
        private bool isEnabled;

        #endregion

        //despues de cada propiedad debe haber un espacio en blanco
        #region Properties

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string CurrentPassword { get; set; }

        //public string Password
        //{
        //    set {
        //        if (password != value)
        //        {
        //            password = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Password"));
        //        }
        //    }
        //    get {
        //        return password;
        //    }
        //}

        public bool IsRunning
        {
            set {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get {
                return isEnabled;
            }
        }

       
        #endregion

        #region Comandos
      
        public ICommand ChangePasswordCommand { get { return new RelayCommand(ChangePassword); } }

        private async void ChangePassword()
        {
            if (string.IsNullOrEmpty(CurrentPassword))
            {
                await dialogService.ShowMessage("Error", "You must enter the current password.");
                return;
            }
            var mainViewModel = MainViewModel.GetInstance();

            if (mainViewModel.CurrentUser.Password != CurrentPassword)
            {
                await dialogService.ShowMessage("Error", "the current password does not match");
                return;
            }
            if (string.IsNullOrEmpty(NewPassword))
            {
                await dialogService.ShowMessage("Error", "You must enter the current password.");
                return;
            }
            if (NewPassword == CurrentPassword)
            {
                await dialogService.ShowMessage("Error", "the current password is the same than current password");
                return;
            }

            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                await dialogService.ShowMessage("Error", "You must enter the current password.");
                return;
            }

            if (NewPassword.Length < 6)
            {
                await dialogService.ShowMessage("Error", "The new password must have at least 6 characters.");
                return;
            }

            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                await dialogService.ShowMessage("Error", "You must enter a password confirm.");
                return;
            }

            if (NewPassword!= ConfirmPassword)
            {
                await dialogService.ShowMessage("Error", "The new password and confirm does not match.");
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            var parameters = dataService.First<Parameter>(false);
            var user = dataService.First<User>(false);
            var request = new ChangePasswordRequest
            {
                CurrentPassword=CurrentPassword,
                Email=user.Email,NewPassword=NewPassword

            };

            var response = await apiService.ChangePassword(parameters.URLBase, "/api", "/Users/ChangePassword",
                user.TokenType, user.AccessToken, request);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                await dialogService.ShowMessage("Error",response.Message);
                return;
            }
            await dialogService.ShowMessage("Confirm", "Your Password has been changed successfully");
            await navigationService.Back();


        }
        #endregion

    }
}
