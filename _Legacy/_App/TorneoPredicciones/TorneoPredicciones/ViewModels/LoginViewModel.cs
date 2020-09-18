using System.ComponentModel;
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
            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService= new DataService();

            //isRunning = false;
            IsEnabled = true;
            IsRemembered = true;
            Email = null;
            Password = null;
        }


        #endregion

        #region Attributes
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private readonly DataService _dataService;
        private readonly NavigationService _navigationService;
        private string _email;
        private string _password;
        private bool _isRunning;
        private bool _isEnabled;
        private bool _isRemembered;
        #endregion

        //despues de cada propiedad debe haber un espacio en blanco
        #region Properties
        public string Email
        {
            set {
                if (_email != value)
                {
                    _email = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Email"));
                }
            }
            get {
                return _email;
            }
        }

        public string Password
        {
            set {
                if (_password != value)
                {
                    _password = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Password"));
                }
            }
            get {
                return _password;
            }
        }

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

        public bool IsRemembered
        {
            set {
                if (_isRemembered != value)
                {
                    _isRemembered = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRemembered"));
                }
            }
            get {
                return _isRemembered;
            }
        }
        #endregion

        #region Comandos
        public ICommand ForgotPasswordCommand { get { return new RelayCommand(ForgotPassword); } }

        private void ForgotPassword()
        {
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.ForgotPassword = new ForgotPasswordViewModel();
            _navigationService.SetMainPage("ForgotPasswordPage");
        }

        public ICommand LoginFacebookCommand { get { return new RelayCommand(LoginFacebook); } }
        
        public ICommand LoginCommand { get { return new RelayCommand(Login); } }

        private async void Login()
        {
            if (string.IsNullOrEmpty(Email))
            {
                await _dialogService.ShowMessage("Error", "You must enter the user email.");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                await _dialogService.ShowMessage("Error", "You must enter a password.");
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            if (!CrossConnectivity.Current.IsConnected)
            {
                IsRunning = false;
                IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
            if (!isReachable)
            {
                IsRunning = false;
                IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var parameters = _dataService.First<Parameter>(false);
         var token = await _apiService.GetToken(parameters.UrlBase, Email, Password);
         //   var token = await apiService.GetToken("http://torneoprediccionesapi.azurewebsites.net", Email, Password);

            if (token == null)
            {
                IsRunning = false;
                IsEnabled = true;
                await _dialogService.ShowMessage("Error", "The user name or password in incorrect.");
                Password = null;
                return;
            }

            if (string.IsNullOrEmpty(token.AccessToken))
            {
                IsRunning = false;
                IsEnabled = true;
                await _dialogService.ShowMessage("Error", token.ErrorDescription);
                Password = null;
                return;
            }

           var response = await _apiService.GetUserByEmail(parameters.UrlBase,
               "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            //var response = await apiService.GetUserByEmail("http://torneoprediccionesapi.azurewebsites.net", 
            //    "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            if (!response.IsSuccess)
            {
                IsRunning = false;
                IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Problem ocurred retrieving user information, try latter.");
                return;
            }


            //IsRunning = false;
            //IsEnabled = true;

            var user = (User)response.Result;

            user.AccessToken = token.AccessToken;
            user.TokenType = token.TokenType;
            user.TokenExpires = token.Expires;
            user.IsRemembered = IsRemembered;
            user.Password = Password;
            _dataService.DeleteAllAndInsert(user.FavoriteTeam);
            _dataService.DeleteAllAndInsert(user.UserType);
            _dataService.DeleteAllAndInsert(user);
            //dataService.DeleteAllAndInsert(user);
            //dataService.InsertOrUpdate(user.FavoriteTeam);
            //dataService.InsertOrUpdate(user.UserType);

            //  await dialogService.ShowMessage("TARAAAAAAAAN!!!!",string.Format("Welcome: {0} {1}, Alias: {2}",user.FirstName,user.LastName,user.NickName));
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.CurrentUser = user;
            mainViewModel.SetCurrentUser(user);
            Email = null;
            // Password = null;
            Password = null;

            IsRunning = false;
            IsEnabled = true;
            mainViewModel.RegisterDevice(); //todo
            _navigationService.SetMainPage("MasterPage");

          

        }

        public ICommand RegisterCommand { get { return new RelayCommand(Register); } }

        private async void Register()
        {
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.NewUser = new NewUserViewModel();
            _navigationService.SetMainPage("NewUserPage");





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

        private void LoginFacebook()
        {
            _navigationService.SetMainPage("LoginFacebookPage");
        }
        #endregion

    }
}
