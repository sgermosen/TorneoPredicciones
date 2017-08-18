using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class NewUserViewModel : INotifyPropertyChanged //una clase solo puede heredar de una clase, pero puede implementar n interfaces
    {

        #region Evento
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private ApiService apiService;
        private DialogService dialogService;
        private DataService dataService;
        private NavigationService navigationService;
        private string email;
        private string password;
        private bool isRunning;
        private bool isEnabled;

        #endregion

        #region Properties

        public ObservableCollection<LeagueItemViewModel> Leagues { get; set; }
        public ObservableCollection<TeamItemViewModel> Teams { get; set; }

        public bool IsRunning
        {
            set {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get {
                return isEnabled;
            }
        }

        
        #endregion

        #region Constructores
        public NewUserViewModel()
        {
            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService = new DataService();
            Leagues = new ObservableCollection<LeagueItemViewModel>();
            Teams = new ObservableCollection<TeamItemViewModel>();
            // IsRunning = false;
            IsEnabled = true;

            LoadLeagues();

        }



        #endregion

        #region Metodos
        private async void LoadLeagues()
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
            IsRunning = true;
            IsEnabled = false;

            var parameters = dataService.First<Parameter>(false);
           // var user = dataService.First<User>(false);
           
            var response = await apiService.Get<League>(parameters.URLBase, "/api", "/Leagues");

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await dialogService.ShowMessage("Error", response.Message);
                return;
            }

            var leagues = (List<League>) response.Result;
            ReloadLeagues(leagues);
        }

        private void ReloadLeagues(List<League> leagues)
        {
            Leagues.Clear();
            foreach (var league in leagues)
            {
                Leagues.Add(new LeagueItemViewModel
                {
                    LeagueId = league.LeagueId,
                    Logo = league.Logo,
                    Name = league.Name,
                    Teams = league.Teams,
                    
                } );

            }
        }

        #endregion

    }
}
