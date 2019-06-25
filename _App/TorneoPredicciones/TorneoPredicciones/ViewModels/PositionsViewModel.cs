using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class PositionsViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private readonly ApiService _apiService;
        private readonly DataService _dataService;
        private readonly DialogService _dialogService;
      //  private NavigationService navigationService;
        private bool _isRefreshing  ;
        private readonly int _tournamentGroupId;
        #endregion

        #region Properties
        public ObservableCollection<TournamentTeamItemViewModel> TournamentTeams { get; set; }

        public bool IsRefreshing
        {
            set {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IsRefreshing"));
                    }
                }
            }
            get {
                return _isRefreshing;
            }
        }
        #endregion

        #region Constructor
        public PositionsViewModel(int tournamentGroupId)
        {
            //instance = this;
            this._tournamentGroupId = tournamentGroupId;
            _apiService = new ApiService();
            _dialogService = new DialogService();
           // navigationService = new NavigationService();
            _dataService = new DataService();

            TournamentTeams = new ObservableCollection<TournamentTeamItemViewModel>();
            LoadTournamentTeams();
        }
  #endregion
       

        #region Methods
        private async void LoadTournamentTeams()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
               // await navigationService.Clear();
                return;
            }
            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("notasti.com");
            if (!isReachable)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                // await navigationService.Clear();
                return;
            }
            _isRefreshing = true;

            var parameters = _dataService.First<Parameter>(false);
            var user = _dataService.First<User>(false);
            var response = await _apiService.Get<TournamentTeam>(parameters.UrlBase, "/api", "/TournamentTeams",
                user.TokenType, user.AccessToken,_tournamentGroupId);
            if (!response.IsSuccess)
            {
                await _dialogService.ShowMessage("Error", response.Message);
              //  await navigationService.Clear();
                return;
            }

            ReloadTournamentTeams((List<TournamentTeam>)response.Result);
        }

        private void ReloadTournamentTeams(List<TournamentTeam> tournamentTeams)
        {
            TournamentTeams.Clear();
            foreach (var tournamentTeam in tournamentTeams)
            {
                TournamentTeams.Add(new TournamentTeamItemViewModel
                {
                    AgainstGoals = tournamentTeam.AgainstGoals,
                    FavorGoals = tournamentTeam.FavorGoals,
                    MatchesLost = tournamentTeam.MatchesLost,
                    MatchesPlayed = tournamentTeam.MatchesPlayed,
                    MatchesTied = tournamentTeam.MatchesTied,
                    MatchesWon = tournamentTeam.MatchesWon,
                    Points = tournamentTeam.Points,
                    Position = tournamentTeam.Position,
                    Team = tournamentTeam.Team,
                    TeamId = tournamentTeam.TeamId,
                    TournamentGroupId = tournamentTeam.TournamentGroupId,
                    TournamentTeamId = tournamentTeam.TournamentTeamId,
                });
            }
        }
        #endregion

        #region Commands
        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        public void Refresh()
        {
           // IsRefreshing = true;
            LoadTournamentTeams();
          //  IsRefreshing = false;
        }
        #endregion



    }
}
