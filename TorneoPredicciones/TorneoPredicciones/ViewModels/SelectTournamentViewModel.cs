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
    public class SelectTournamentViewModel : INotifyPropertyChanged
    {
        #region Attributes
        private ApiService apiService;
        private DataService dataService;
        private DialogService dialogService;
        private NavigationService navigationService;
        private bool isRefreshing = false;
        #endregion

        #region Properties
        public ObservableCollection<TournamentItemViewModel> Tournaments { get; set; }

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
        public SelectTournamentViewModel()
        {
           // instance = this;

            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService = new DataService();

            Tournaments = new ObservableCollection<TournamentItemViewModel>();
            LoadTournaments();
        }
        #endregion

        #region Singleton
        private static SelectTournamentViewModel instance;

        public static SelectTournamentViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new SelectTournamentViewModel();
            }

            return instance;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private async void LoadTournaments()
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
            var response = await apiService.Get<Tournament>(parameter.URLBase, "/api", "/Tournaments", user.TokenType, user.AccessToken);
            if (!response.IsSuccess)
            {
                await dialogService.ShowMessage("Error", response.Message);
               // await navigationService.Clear();
                return;
            }

            ReloadTournaments((List<Tournament>)response.Result);
        }

        private void ReloadTournaments(List<Tournament> tournaments)
        {
            Tournaments.Clear();
            foreach (var tournament in tournaments)
            {
                Tournaments.Add(new TournamentItemViewModel
                {
                    Dates = tournament.Dates,
                    Groups = tournament.Groups,
                    Logo = tournament.Logo,
                    Name = tournament.Name,
                    TournamentId = tournament.TournamentId,
                });
            }
        }
        #endregion

        #region Commands
        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        public void Refresh()
        {
          //  IsRefreshing = true;
            LoadTournaments();
          //  IsRefreshing = false;
        }
        #endregion
    }

    //public  class SelectTournamentViewModel: INotifyPropertyChanged
    //  {




    //      #region Propidades


    //      //origen de datos del listview
    //      public ObservableCollection<TournamentItemViewModel> Tournaments { get; set; }

    //      public bool IsRefreshing
    //      {
    //          set {
    //              if (isRefreshing != value)
    //              {
    //                  isRefreshing = value;
    //                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRefreshing"));
    //              }
    //          }
    //          get {
    //              return isRefreshing;
    //          }
    //      }

    //      #endregion

    //      #region Contructores

    //      public SelectTournamentViewModel()
    //      {
    //          apiService=new ApiService();
    //          dialogService=new DialogService();
    //          navigationService=new NavigationService();
    //          dataService=new DataService();

    //          Tournaments= new ObservableCollection<TournamentItemViewModel>();
    //          LoadTournaments();

    //      }



    //      #endregion

    //      #region Metodos
    //      private async void LoadTournaments()
    //      {
    //          if (!CrossConnectivity.Current.IsConnected)
    //          {
    //             // IsRunning = false;
    //             // IsEnabled = true;
    //              await dialogService.ShowMessage("Error", "Check you internet connection.");
    //              return;
    //          }

    //          var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
    //          if (!isReachable)
    //          {
    //             // IsRunning = false;
    //             // IsEnabled = true;
    //              await dialogService.ShowMessage("Error", "Check you internet connection.");
    //              return;
    //          }
    //          IsRefreshing = true;
    //          var parameters = dataService.First<Parameter>(false);
    //          var user =   dataService.First<User>(false);
    //          var response = await apiService.Get<Tournament>(parameters.URLBase, "/api", "/Tournaments",
    //              user.TokenType,
    //              user.AccessToken);
    //          IsRefreshing = false;
    //          if (!response.IsSuccess)
    //          {
    //              // IsRunning = false;
    //              // IsEnabled = true;
    //              await dialogService.ShowMessage("Error", response.Message);
    //              return;
    //          }

    //          ReloadTournaments((List<Tournament>)response.Result);

    //      }

    //      private void ReloadTournaments(List<Tournament> tournaments)
    //      {
    //          Tournaments.Clear();
    //          foreach (var tournament in tournaments )
    //          {
    //              Tournaments.Add(new TournamentItemViewModel 
    //              {
    //                  Dates = tournament.Dates,
    //                  Groups=tournament.Groups,
    //                  Logo=tournament.Logo,
    //                  Name=tournament.Name,
    //                  TournamentId=tournament.TournamentId
    //              });

    //          }
    //      }

    //      #endregion

    //      #region Atributos
    //      private ApiService apiService;
    //      private DialogService dialogService;
    //      private NavigationService navigationService;
    //      private DataService dataService;
    //      //private string email;
    //      //private string password;
    //      private bool isRefreshing;
    //      //private bool isEnabled;
    //      //private bool isRemembered;


    //      #endregion

    //      #region Eventos
    //      public event PropertyChangedEventHandler PropertyChanged; //implementamso porque cambiare objetos en tiempo de ejecucion
    //      #endregion

    //      #region Comandos
    //      public ICommand RefreshCommand { get {return new RelayCommand(Refresh);} }

    //      private void Refresh()
    //      {
    //          LoadTournaments();
    //      }


    //      #endregion

    //  }
}
