using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using TorneoPredicciones.Interfaces;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;
using Xamarin.Forms;
using Point = TorneoPredicciones.Models.Point;

namespace TorneoPredicciones.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Eventos
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Atriutos
        private ApiService apiService;
        private DataService dataService;
       
        private User currentUser;
        #endregion

        #region Singleton
        private static MainViewModel instance;

        public static MainViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new MainViewModel();
            }

            return instance;
        }
        #endregion
        //notas, siempre se crea una pagina, luego una viewmodel y despues se instancia en la MainViewModel (osea yo) para asi asociar 
        // usando el patron Locator, en la page, el binding principal, el nombre que pongamos aqui es el nombre al cual bindamos la pagina
        // que a su vez envia a bindar a la viwemodel correspondiente
        //recordar especificar en el loadmenu el nombre de la pagina y configurar en el MenuItemViewModel hacia donde va a navegar
        //pues es en el menuitemviewmodel donde se instancian los viewmodel (new) usando este patron
        //de igual forma en el navigationservice se debe crear en la funcion navigate hacia donde va pinchada la accion
        #region Properties
        public ProfileViewModel Profile { get; set; }
        public LoginViewModel Login { get; set; }
        public SelectTournamentViewModel SelectTournament { get; set; }
        public SelectMatchViewModel SelectMatch { get; set; }
        public EditPredictionViewModel EditPrediction { get; set; }
        public NewUserViewModel NewUser { get; set; }
        public SelectGroupViewModel SelectGroup { get; set; }
        public ChangePasswordViewModel ChangePassword { get; set; }
        public ConfigViewModel Config { get; set; }
        public PositionsViewModel Positions { get; set; }
        
        public UsersGroupViewModel UsersGroup { get; set; }
        public SelectUserGroupViewModel SelectUserGroup { get; set; }
        public ObservableCollection<MenuItemViewModel> Menu { get; set; }

        //public User CurrentUser { get; set; }
        public User CurrentUser
        {
            set {
                if (currentUser != value)
                {
                    currentUser = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentUser"));
                }
            }
            get {
                return currentUser;
            }
        }


        #endregion

        #region Constructores

        public MainViewModel()
        {
            instance = this;
            Login = new LoginViewModel();
            apiService= new ApiService();
            dataService= new DataService();
            LoadMenu();
        }

        #endregion

        #region Comandos

        public ICommand RefreshPointsCommand { get {return new RelayCommand(RefreshPoints);}  }

        private async  void RefreshPoints()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                return; 
            }
            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("notasti.com");
            if (!isReachable)
            {
                return;
            }
            
            var parameters = dataService.First<Parameter>(false);
            var user = dataService.First<User>(false);
            var response = await apiService.GetPoints(parameters.URLBase, "/api", "/Users/GetPoints", user.TokenType, user.AccessToken,user.UserId);

            if (!response.IsSuccess)
            {
                return;
            }
            var point = (Point) response.Result;
            if (CurrentUser.Points != point.Points)
            {
                CurrentUser.Points = point.Points;
                dataService.Update(CurrentUser); //actualizamos la base de datos local
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentUser"));
            }
           
        }

        #endregion

        #region Metodos
        public void RegisterDevice()
        {
            var register = DependencyService.Get<IRegisterDevice>();
            register.RegisterDevice();
        }

        private void LoadMenu()
        {
            Menu = new ObservableCollection<MenuItemViewModel>();

            Menu.Add(new MenuItemViewModel
            {
                Icon = "predictions.png",
                PageName = "SelectTournamentPage",
                Title = "Predictions",
            });

            Menu.Add(new MenuItemViewModel
            {
                Icon = "groups.png",
                PageName = "SelectUserGroupPage",
                Title = "Groups",
            });

            Menu.Add(new MenuItemViewModel
            {
                Icon = "tournaments.png",
                PageName = "TournamentsPage",
                Title = "Tournaments",
            });

            Menu.Add(new MenuItemViewModel
            {
                Icon = "myresults.png",
                PageName = "ResultsPage",
                Title = "My Results",
            });

            Menu.Add(new MenuItemViewModel
            {
                Icon = "config.png",
                PageName = "ConfigPage",
                Title = "Config",
            });

            Menu.Add(new MenuItemViewModel
            {
                Icon = "logut.png",
                PageName = "LoginPage",
                Title = "Logut",
            });
        }



        #endregion

    }
}
