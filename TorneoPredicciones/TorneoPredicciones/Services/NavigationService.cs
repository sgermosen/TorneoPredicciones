using System.Threading.Tasks;
using TorneoPredicciones.Pages;

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
                case "SelectGroupPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new SelectGroupPage());
                    break;
                case "PositionsPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new PositionsPage());
                    break;
                case "ConfigPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new ConfigPage());
                    break;
                case "ChangePasswordPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new ChangePasswordPage());
                    break;
                case "SelectUserGroupPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new SelectUserGroupPage());
                    break;
                case "UsersGroupPage":
                    // mainViewModel.SelectTournament = new SelectTournamentViewModel();
                    await App.Navigator.PushAsync(new UsersGroupPage());
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
                case "NewUserPage":
                    // Logout();
                    App.Current.MainPage = new NewUserPage();
                    break;
                case "LoginFacebookPage":
                    // Logout();
                    App.Current.MainPage = new LoginFacebookPage();
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
