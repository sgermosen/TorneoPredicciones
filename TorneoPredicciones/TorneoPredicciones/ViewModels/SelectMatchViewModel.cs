using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Services;
using System.Collections.ObjectModel;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using Xamarin.Forms.Xaml;

namespace TorneoPredicciones.ViewModels
{
    public class SelectMatchViewModel:INotifyPropertyChanged
    {
         
        #region Propidades


        //origen de datos del listview
        public ObservableCollection<MatchItemViewModel> Matches { get; set; }

        public bool IsRefreshing
        {
            set {
                if (isRefreshing != value)
                {
                    isRefreshing = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRefreshing"));
                }
            }
            get {
                return isRefreshing;
            }
        }

        #endregion

        #region Contructores

        public SelectMatchViewModel(int tournamentId)
        {
            this.tournamentId = tournamentId;

            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService = new DataService();
           
            Matches= new ObservableCollection<MatchItemViewModel>();

            LoadMatches();
        }

     

        #endregion

        #region Atributos
        private int tournamentId;

        private ApiService apiService;
        private DialogService dialogService;
        private NavigationService navigationService;
        private DataService dataService;
        //private string email;
        //private string password;
        private bool isRefreshing;
        //private bool isEnabled;
        //private bool isRemembered;


        #endregion

        #region Eventos
        public event PropertyChangedEventHandler PropertyChanged; //implementamso porque cambiare objetos en tiempo de ejecucion
        #endregion

        #region Comandos
        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        private void Refresh()
        {
            //LoadMatch();
        }


        #endregion

        #region Metodos
        private async void LoadMatches()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
            if (!isReachable)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }
            IsRefreshing = true;
            var parameters = dataService.First<Parameter>(false);
            var user = dataService.First<User>(false);
            var controller = string.Format("/Tournaments/GetMatchesToPredict/{0}/{1}",tournamentId,user.UserId);

            var response = await apiService.Get<Match>(parameters.URLBase, "/api", controller,
                user.TokenType,
                user.AccessToken);
            IsRefreshing = false;
            if (!response.IsSuccess)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await dialogService.ShowMessage("Error", response.Message);
                return;
            }

            ReloadMatches((List<Match>)response.Result);
        }

        private void ReloadMatches(List<Match> matches)
        {
           Matches.Clear();
            foreach (var match in matches)
            {
                Matches.Add(new MatchItemViewModel
                {
                    DateId= match.DateId,
                    DateTime = match.DateTime,
                    Local = match.Local,
                    LocalGoals= match.LocalGoals,
                    LocalId= match.LocalId,
                    MatchId= match.MatchId,
                    StatusId= match.StatusId,
                    TournamentGroupId= match.TournamentGroupId,
                    Visitor= match.Visitor,
                    VisitorGoals= match.VisitorGoals,
                    VisitorId= match.VisitorId,
                    WasPredicted= match.WasPredicted

                });
            }
        }

        #endregion


    }
}
