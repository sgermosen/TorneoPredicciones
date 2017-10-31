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

        private DataService dataService;

        #endregion


        #region Propiedades
        public static NavigationPage Navigator { get; internal set; }
        public static MasterPage Master { get; set; }

        #endregion

        #region Constructores

        

        #endregion
        public App()
        {
            InitializeComponent();
            dataService= new DataService();
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

        #region Methods
        public static Action HideLoginView
        {
            get {
                return new Action(() => App.Current.MainPage = new LoginPage());
            }
        }

        public static void NavigateToProfile(FacebookResponse profile)
        {
            //var profileViewModel = new ProfileViewModel(profile);
            //var mainViewModel = MainViewModel.GetInstance();
            //mainViewModel.Profile = profileViewModel;
            //App.Current.MainPage = new ProfilePage();
            
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.Profile = new ProfileViewModel(profile);
            App.Current.MainPage = new ProfilePage();
        }

        private void LoadParameters()
        {
            var urlBase = Application.Current.Resources["URLBase"].ToString();
            var urlBase2 = Application.Current.Resources["URLBase2"].ToString();
            var parameter = dataService.First<Parameter>(false);
            if (parameter == null)
            {
                parameter = new Parameter
                {
                    URLBase = urlBase,
                    URLBase2=urlBase2
                };

                dataService.Insert(parameter);
            }
            else
            {
                parameter.URLBase = urlBase;
                parameter.URLBase2 = urlBase2;
                dataService.Update(parameter);
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
