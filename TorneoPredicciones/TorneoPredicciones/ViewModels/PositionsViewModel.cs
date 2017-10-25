using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class PositionsViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private ApiService apiService;
        private DataService dataService;
        private DialogService dialogService;
      //  private NavigationService navigationService;
        private bool isRefreshing = false;
        private int tournamentGroupId;
        #endregion

        #region Properties
        public ObservableCollection<TournamentTeamItemViewModel> TournamentTeams { get; set; }

        public bool IsRefreshing
        {
            set {
                if (isRefreshing != value)
                {
                    isRefreshing = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IsRefreshing"));
                    }
                }
            }
            get {
                return isRefreshing;
            }
        }
        #endregion

        #region Constructor
        public PositionsViewModel(int tournamentGroupId)
        {
            instance = this;
            this.tournamentGroupId = tournamentGroupId;
            apiService = new ApiService();
            dialogService = new DialogService();
           // navigationService = new NavigationService();
            dataService = new DataService();

            TournamentTeams = new ObservableCollection<TournamentTeamItemViewModel>();
            LoadTournamentTeams();
        }
  #endregion
       

        #region Methods
        private async void LoadTournamentTeams()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await dialogService.ShowMessage("Error", "Check you internet connection.");
               // await navigationService.Clear();
                return;
            }
            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("notasti.com");
            if (!isReachable)
            {
                await dialogService.ShowMessage("Error", "Check you internet connection.");
                // await navigationService.Clear();
                return;
            }
            isRefreshing = true;

            var parameter = dataService.First<Parameter>(false);
            var user = dataService.First<User>(false);
            var response = await apiService.Get<TournamentTeam>(parameter.URLBase, "/api", "/TournamentTeams", user.TokenType, user.AccessToken,tournamentGroupId);
            if (!response.IsSuccess)
            {
                await dialogService.ShowMessage("Error", response.Message);
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
