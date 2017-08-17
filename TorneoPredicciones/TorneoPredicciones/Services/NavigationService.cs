using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoPredicciones.Models;
using TorneoPredicciones.Pages;
using TorneoPredicciones.ViewModels;

namespace TorneoPredicciones.Services
{
    public class NavigationService
    {
        #region Attributes
        // private DataService dataService;
        #endregion

        #region Constructors
        //public NavigationService()
        //{
        //    dataService = new DataService();
        //}
        #endregion

        #region Methods

        public async Task Navigate(string pageName)
        {
            App.Master.IsPresented = false; //para que oculte la master detail
            //var mainViewModel = MainViewModel.GetInstance();

            switch (pageName)
            {
                //case "SelectGroupPage":
                //    await App.Navigator.PushAsync(new SelectGroupPage());
                //    break;
                case "SelectTournamentPage":
                   // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new SelectTournamentPage());
                    break;
                case "SelectMatchPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new SelectMatchPage());
                    break;
                case "EditPredictionPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new EditPredictionPage());
                    break;
                default:
                    break;
            }
        }

        public void SetMainPage(string pageName)
        {
            switch (pageName)
            {
                case "MasterPage":
                    App.Current.MainPage = new MasterPage();
                    break;
                case "LoginPage":
                   // Logout();
                    App.Current.MainPage = new LoginPage();
                    break;
                default:
                    break;
            }
        }

        //private void Logout()
        //{
        //    var user = dataService.First<User>(false);
        //    if (user != null)
        //    {
        //        user.IsRemembered = false;
        //        dataService.Update(user);
        //    }
        //}

        public async Task Back()
        {
            await App.Navigator.PopAsync();//desapilar (quitar la capa)
        }

        public async Task Clear()
        {
            await App.Navigator.PopToRootAsync();
        }


        #endregion
    }

}
