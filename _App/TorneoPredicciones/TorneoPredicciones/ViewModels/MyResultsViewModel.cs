using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class MyResultsViewModel : INotifyPropertyChanged
    {
      

        public MyResultsViewModel(int tournamentGroupId)
        {
            this._tournamentGroupId = tournamentGroupId;

            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService = new DataService();

            Results = new ObservableCollection<ResultItemViewModel>();

               LoadResults(); //como lo llamo en el onapearing no lo necesito aca
        }

       

        #region Propidades

        //origen de datos del listview
        public ObservableCollection<ResultItemViewModel> Results { get; set; }

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

        public string Filter
        {
            set {
                if (_filter == value) return;
                _filter = value;
                if (string.IsNullOrEmpty(_filter))
                {
                    ReloadResults(_results);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Filter"));
            }
            get {
                return _filter;
            }
        }
        #endregion

        #region Contructores





        #endregion

        #region Atributos

        //private readonly int _tournamentId;
        private readonly int _tournamentGroupId;
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private NavigationService _navigationService;
        private readonly DataService _dataService;
        private bool _isRefreshing;
        private string _filter;
        private List<Result> _results;

        #endregion

        #region Eventos
        public event PropertyChangedEventHandler PropertyChanged; //implementamso porque cambiare objetos en tiempo de ejecucion
        #endregion

        #region Comandos
        public ICommand SearchResultCommand { get { return new RelayCommand(SearchResult); } }

        private void SearchResult()
        {
            //var list = results.Where(r => r.Match.Local.Initials == filter);
            var list = _results
                .Where(r => r.Match.Local.Initials.ToUpper().Contains(_filter.ToUpper()) ||
                            r.Match.Visitor.Initials.ToUpper().Contains(_filter.ToUpper())
                ).ToList();
            ReloadResults(list);
        }


        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        private void Refresh()
        {
            LoadResults();
            //IsRefreshing = true;
            //LoadTournaments();
            //IsRefreshing = false;

        }


        #endregion

        #region Metodos

        
        private async void LoadResults()
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
            var controller = string.Format("/Tournaments/GetResults/{0}/{1}", _tournamentGroupId, user.UserId);

            var response = await _apiService.Get<Result>(parameters.UrlBase, "/api", controller,
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

            _results = (List<Result>) response.Result;
            ReloadResults(_results);
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

        private void ReloadResults(List<Result> results)
        {
            Results.Clear();
            foreach (var result in results)
            {
                Results.Add(new ResultItemViewModel()
                {
                    LocalGoals = result.LocalGoals,
                    Match = result.Match,
                    MatchId = result.MatchId,
                    Points = result.Points,
                    PredictionId = result.PredictionId,
                    UserId = result.UserId,
                    VisitorGoals = result.VisitorGoals,
                });
            }
        }

        #endregion
    }
}
