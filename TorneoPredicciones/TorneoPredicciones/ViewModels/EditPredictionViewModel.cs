using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class EditPredictionViewModel : Match, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private ApiService apiService;
        private DialogService dialogService;
        private DataService dataService;
        private NavigationService navigationService;
        private Match match;
        private bool isRunning;
        private bool isEnabled;
        #endregion

        #region Properties
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

        public string GoalsLocal { get; set; }
        public string GoalsVisitor { get; set; }
        #endregion

        #region Constructor
        public EditPredictionViewModel(Match match)
        {
            this.match = match;

            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService = new DataService();

            

            DateId = match.DateId;
            DateTime = match.DateTime;
            Local = match.Local;
            LocalGoals = match.LocalGoals;
            LocalId = match.LocalId;
            MatchId = match.MatchId;
            StatusId = match.StatusId;
            TournamentGroupId = match.TournamentGroupId;
            Visitor = match.Visitor;
            VisitorGoals = match.VisitorGoals;
            VisitorId = match.VisitorId;
            WasPredicted = match.WasPredicted;
            GoalsLocal = LocalGoals2.ToString();
            GoalsVisitor = VisitorGoals2.ToString();

            IsEnabled = true;
        }
        #endregion

        #region Comandos 

        public ICommand SaveCommand { get { return new RelayCommand(Save); } }


        private async void Save()
        {
            if (string.IsNullOrEmpty(GoalsLocal))
            {
                await dialogService.ShowMessage("Error", "Necesitas ingresar una prediccion para el Local");
                return;
            }
            if (string.IsNullOrEmpty(GoalsVisitor))
            {
                await dialogService.ShowMessage("Error", "Necesitas ingresar una prediccion para el Visitante");
                return;
            }

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
            var user = dataService.First<User>(false);

            var prediction = new Prediction
            {
                LocalGoals= int.Parse(GoalsLocal),
                MatchId=MatchId,
                Points=0,
                UserId=user.UserId,
                VisitorGoals = int.Parse(GoalsVisitor),
            };

           
            var response = await apiService.Post(parameters.URLBase, "/api", "/Predictions",
                user.TokenType,
                user.AccessToken,prediction);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await dialogService.ShowMessage("Error", "Ha ocurrido un error al momento de guardar la prediccion, intente luego");
                return;
            }
            await navigationService.Back();
            //if (LocalGoals2 == null)
            //{
            //    await dialogService.ShowMessage("Error","Necesitas ingresar una prediccion para el Local");
            //    return;
            //}
        }
        #endregion
    }

}
