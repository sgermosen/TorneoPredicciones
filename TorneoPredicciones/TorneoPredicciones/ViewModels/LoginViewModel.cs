using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        #region Eventos
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructores

        public LoginViewModel()
        {
            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService= new DataService();

            //isRunning = false;
            IsEnabled = true;
            IsRemembered = true;

        }


        #endregion

        #region Attributes
        private ApiService apiService;
        private DialogService dialogService;
        private DataService dataService;
         private NavigationService navigationService;
        private string email;
        private string password;
        private bool isRunning;
        private bool isEnabled;
        private bool isRemembered;
        #endregion

        //despues de cada propiedad debe haber un espacio en blanco
        #region Properties
        public string Email
        {
            set {
                if (email != value)
                {
                    email = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Email"));
                }
            }
            get {
                return email;
            }
        }

        public string Password
        {
            set {
                if (password != value)
                {
                    password = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Password"));
                }
            }
            get {
                return password;
            }
        }

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

        public bool IsRemembered
        {
            set {
                if (isRemembered != value)
                {
                    isRemembered = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRemembered"));
                }
            }
            get {
                return isRemembered;
            }
        }
        #endregion

        #region Comandos
        public ICommand LoginCommand { get { return new RelayCommand(Login); } }

        private async void Login()
        {
            if (string.IsNullOrEmpty(Email))
            {
                await dialogService.ShowMessage("Error", "You must enter the user email.");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                await dialogService.ShowMessage("Error", "You must enter a password.");
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            if (!CrossConnectivity.Current.IsConnected)
            {
                IsRunning = false;
                IsEnabled = true;
                await dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
            if (!isReachable)
            {
                IsRunning = false;
                IsEnabled = true;
                await dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var parameters = dataService.First<Parameter>(false);
         var token = await apiService.GetToken(parameters.URLBase, Email, Password);
         //   var token = await apiService.GetToken("http://torneoprediccionesapi.azurewebsites.net", Email, Password);

            if (token == null)
            {
                IsRunning = false;
                IsEnabled = true;
                await dialogService.ShowMessage("Error", "The user name or password in incorrect.");
                Password = null;
                return;
            }

            if (string.IsNullOrEmpty(token.AccessToken))
            {
                IsRunning = false;
                IsEnabled = true;
                await dialogService.ShowMessage("Error", token.ErrorDescription);
                Password = null;
                return;
            }

           var response = await apiService.GetUserByEmail(parameters.URLBase,
               "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            //var response = await apiService.GetUserByEmail("http://torneoprediccionesapi.azurewebsites.net", 
            //    "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            if (!response.IsSuccess)
            {
                IsRunning = false;
                IsEnabled = true;
                await dialogService.ShowMessage("Error", "Problem ocurred retrieving user information, try latter.");
                return;
            }

            Email = null;
            Password = null;

            IsRunning = false;
            IsEnabled = true;
            var user = (User)response.Result;

            user.AccessToken = token.AccessToken;
            user.TokenType = token.TokenType;
            user.TokenExpires = token.Expires;
            user.IsRemembered = IsRemembered;
            user.Password = Password;
            dataService.DeleteAllAndInsert(user.FavoriteTeam);
            dataService.DeleteAllAndInsert(user.UserType);
            dataService.DeleteAllAndInsert(user);
            //dataService.DeleteAllAndInsert(user);
            //dataService.InsertOrUpdate(user.FavoriteTeam);
            //dataService.InsertOrUpdate(user.UserType);

            //  await dialogService.ShowMessage("TARAAAAAAAAN!!!!",string.Format("Welcome: {0} {1}, Alias: {2}",user.FirstName,user.LastName,user.NickName));
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.CurrentUser = user;
            navigationService.SetMainPage("MasterPage");
        }

        public ICommand RegisterCommand { get { return new RelayCommand(Register); } }

        private async void Register()
        {
            var mainViewModel = MainViewModel.GetInstance();
            navigationService.SetMainPage("NewUserPage");





            //if (string.IsNullOrEmpty(Email))
            //{
            //    await dialogService.ShowMessage("Error", "You must enter the user email.");
            //    return;
            //}

            //if (string.IsNullOrEmpty(Password))
            //{
            //    await dialogService.ShowMessage("Error", "You must enter a password.");
            //    return;
            //}

            //IsRunning = true;
            //IsEnabled = false;

            //if (!CrossConnectivity.Current.IsConnected)
            //{
            //    IsRunning = false;
            //    IsEnabled = true;
            //    await dialogService.ShowMessage("Error", "Check you internet connection.");
            //    return;
            //}

            //var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
            //if (!isReachable)
            //{
            //    IsRunning = false;
            //    IsEnabled = true;
            //    await dialogService.ShowMessage("Error", "Check you internet connection.");
            //    return;
            //}

            //var parameters = dataService.First<Parameter>(false);
            //var token = await apiService.GetToken(parameters.URLBase, Email, Password);
            ////   var token = await apiService.GetToken("http://torneoprediccionesapi.azurewebsites.net", Email, Password);

            //if (token == null)
            //{
            //    IsRunning = false;
            //    IsEnabled = true;
            //    await dialogService.ShowMessage("Error", "The user name or password in incorrect.");
            //    Password = null;
            //    return;
            //}

            //if (string.IsNullOrEmpty(token.AccessToken))
            //{
            //    IsRunning = false;
            //    IsEnabled = true;
            //    await dialogService.ShowMessage("Error", token.ErrorDescription);
            //    Password = null;
            //    return;
            //}

            //var response = await apiService.GetUserByEmail(parameters.URLBase,
            //    "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            ////var response = await apiService.GetUserByEmail("http://torneoprediccionesapi.azurewebsites.net", 
            ////    "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            //if (!response.IsSuccess)
            //{
            //    IsRunning = false;
            //    IsEnabled = true;
            //    await dialogService.ShowMessage("Error", "Problem ocurred retrieving user information, try latter.");
            //    return;
            //}

            //Email = null;
            //Password = null;

            //IsRunning = false;
            //IsEnabled = true;
            //var user = (User)response.Result;

            //user.AccessToken = token.AccessToken;
            //user.TokenType = token.TokenType;
            //user.TokenExpires = token.Expires;
            //user.IsRemembered = IsRemembered;
            //user.Password = Password;
            //dataService.DeleteAllAndInsert(user.FavoriteTeam);
            //dataService.DeleteAllAndInsert(user.UserType);
            //dataService.DeleteAllAndInsert(user);
            ////dataService.DeleteAllAndInsert(user);
            ////dataService.InsertOrUpdate(user.FavoriteTeam);
            ////dataService.InsertOrUpdate(user.UserType);

            ////  await dialogService.ShowMessage("TARAAAAAAAAN!!!!",string.Format("Welcome: {0} {1}, Alias: {2}",user.FirstName,user.LastName,user.NickName));
            //var mainViewModel = MainViewModel.GetInstance();
            //mainViewModel.CurrentUser = user;
            //navigationService.SetMainPage("MasterPage");
        }
        #endregion

    }
}
