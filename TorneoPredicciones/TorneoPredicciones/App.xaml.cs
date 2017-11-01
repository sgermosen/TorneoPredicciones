using System;
using TorneoPredicciones.Classes;
using TorneoPredicciones.Models;
using TorneoPredicciones.Pages;
using TorneoPredicciones.Services;
using TorneoPredicciones.ViewModels;
using Xamarin.Forms;

namespace TorneoPredicciones
{
    public partial class App : Application
    {
        #region Atributos

        private   DataService dataService;
       

        #endregion


        #region Propiedades
        public static NavigationPage Navigator { get; internal set; }
        public static MasterPage Master { get; set; }


        #endregion

        #region Constructores
        public App()
        {
            InitializeComponent();

            dataService = new DataService();
            //apiService= new ApiService();
            //dialogService= new DialogService();
            //navigationService = new NavigationService();

            LoadParameters();

            var user = dataService.First<User>(false);

            if (user != null && user.IsRemembered && user.TokenExpires > DateTime.Now)
            {
                var favoriteTeam = dataService.Find<Team>(user.FavoriteTeamId, false);
                user.FavoriteTeam = favoriteTeam;
                var mainViewModel = MainViewModel.GetInstance();
                mainViewModel.CurrentUser = user;
                mainViewModel.RegisterDevice();//todo
                MainPage = new MasterPage();
            }
            else
            {
                MainPage = new LoginPage();
            }


            //   MainPage = new LoginPage();
        }


        #endregion


        #region Methods
        public static   Action HideLoginView
        {
            get {
                return new Action(() => App.Current.MainPage = new LoginPage());
            }
        }

        public static async   void NavigateToProfile(FacebookResponse profile)
        {
            //var profileViewModel = new ProfileViewModel(profile);
            //var mainViewModel = MainViewModel.GetInstance();
            //mainViewModel.Profile = profileViewModel;
            //App.Current.MainPage = new ProfilePage();

            //var mainViewModel = MainViewModel.GetInstance();
            //mainViewModel.Profile = new ProfileViewModel(profile);
            //App.Current.MainPage = new ProfilePage();

            var apiService = new  ApiService();
            var  dialogService = new DialogService();
            var navigationService = new NavigationService();
            var  dataService = new DataService();
        var parameters =   dataService.First<Parameter>(false);
            var token = await apiService.LoginFacebook(parameters.URLBase, "/api", "/Users/LoginFacebook", profile);

            if (token == null)
            {
             App.Current.MainPage = new LoginPage();
                return;
            }

            var response = await apiService.GetUserByEmail(parameters.URLBase,
                "/api", "/Users/GetUserByEmail", token.TokenType, token.AccessToken, token.UserName);

            if (!response.IsSuccess)
            {
                await dialogService.ShowMessage("Error",
                    "Hubo un problema trayendo la informacion del usuario, intente mas tarde");
                return;

            }

            var user = (User)response.Result;

            user.AccessToken = token.AccessToken;
            user.TokenType = token.TokenType;
            user.TokenExpires = token.Expires;
            user.IsRemembered = true;
            user.Password = profile.Id;
            dataService.DeleteAllAndInsert(user.FavoriteTeam);
            dataService.DeleteAllAndInsert(user.UserType);
            dataService.DeleteAllAndInsert(user);
            //dataService.DeleteAllAndInsert(user);
            //dataService.InsertOrUpdate(user.FavoriteTeam);
            //dataService.InsertOrUpdate(user.UserType);

            //  await dialogService.ShowMessage("TARAAAAAAAAN!!!!",string.Format("Welcome: {0} {1}, Alias: {2}",user.FirstName,user.LastName,user.NickName));
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.CurrentUser = user;
            mainViewModel.RegisterDevice(); //todo
            navigationService.SetMainPage("MasterPage");

        }

        private void LoadParameters()
        {
            var urlBase = Application.Current.Resources["URLBase"].ToString();
            var urlBase2 = Application.Current.Resources["URLBase2"].ToString();
            var parameters = dataService.First<Parameter>(false);
            if (parameters == null)
            {
                parameters = new Parameter
                {
                    URLBase = urlBase,
                    URLBase2=urlBase2
                };

                dataService.Insert(parameters);
            }
            else
            {
                parameters.URLBase = urlBase;
                parameters.URLBase2 = urlBase2;
                dataService.Update(parameters);
            }
        }



        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion
    }
}
