using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Services;
using System.Collections.ObjectModel;
using Plugin.Connectivity;
using TorneoPredicciones.Models;

namespace TorneoPredicciones.ViewModels
{
    public class SelectMatchViewModel:INotifyPropertyChanged
    {
        #region Singleton
        private static SelectMatchViewModel _instance;

        public static SelectMatchViewModel GetInstance()
        {
            return _instance;
        }
        #endregion

        #region Propidades


        //origen de datos del listview
        public ObservableCollection<MatchItemViewModel> Matches { get; set; }

        public bool IsRefreshing
        {
            set {
                if (_isRefreshing == value) return;
                _isRefreshing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRefreshing"));
            }
            get {
                return _isRefreshing;
            }
        }

        #endregion

        #region Contructores

        public SelectMatchViewModel(int tournamentId)
        {
            
            this._tournamentId = tournamentId;
            _instance = this;

            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService = new DataService();
           
            Matches= new ObservableCollection<MatchItemViewModel>();

        //    LoadMatches(); como lo llamo en el onapearing no lo necesito aca

        }

     

        #endregion

        #region Atributos

        private readonly int _tournamentId;
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private NavigationService _navigationService;
        private readonly DataService _dataService;
        //private string email;
        //private string password;
        private bool _isRefreshing;
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
            LoadMatches();
            //IsRefreshing = true;
            //LoadTournaments();
            //IsRefreshing = false;

        }


        #endregion

        #region Metodos
        private async void LoadMatches()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
            if (!isReachable)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }
            IsRefreshing = true;
            var parameters = _dataService.First<Parameter>(false);
            var user = _dataService.First<User>(false);
            var controller = string.Format("/Tournaments/GetMatchesToPredict/{0}/{1}",_tournamentId,user.UserId);

            var response = await _apiService.Get<Match>(parameters.UrlBase, "/api", controller,
                user.TokenType,
                user.AccessToken);
            IsRefreshing = false;
            if (!response.IsSuccess)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await _dialogService.ShowMessage("Error", response.Message);
                return;
            }

            ReloadMatches((List<Match>)response.Result);
        }

        //private void ReloadTournaments(List<Tournament> tournaments)
        //{
        //    Tournaments.Clear();
        //    foreach (var tournament in tournaments)
        //    {
        //        Tournaments.Add(new TournamentItemViewModel
        //        {
        //            Dates = tournament.Dates,
        //            Groups = tournament.Groups,
        //            Logo = tournament.Logo,
        //            Name = tournament.Name,
        //            TournamentId = tournament.TournamentId,
        //        });
        //    }
        //}

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
