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
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private readonly DataService _dataService;
        private readonly NavigationService _navigationService;
        private Match _match;
        private bool _isRunning;
        private bool _isEnabled;
        #endregion

        #region Properties
        public bool IsRunning
        {
            set {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get {
                return _isRunning;
            }
        }

        public bool IsEnabled
        {
            set {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get {
                return _isEnabled;
            }
        }

        public string GoalsLocal { get; set; }
        public string GoalsVisitor { get; set; }
        #endregion

        #region Constructor
        public EditPredictionViewModel(Match match)
        {
            this._match = match;

            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService = new DataService();

            

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
                await _dialogService.ShowMessage("Error", "Necesitas ingresar una prediccion para el Local");
                return;
            }
            if (string.IsNullOrEmpty(GoalsVisitor))
            {
                await _dialogService.ShowMessage("Error", "Necesitas ingresar una prediccion para el Visitante");
                return;
            }

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
            IsRunning = true;
            IsEnabled = false;

            var parameters = _dataService.First<Parameter>(false);
            var user = _dataService.First<User>(false);

            var prediction = new Prediction
            {
                LocalGoals= int.Parse(GoalsLocal),
                MatchId=MatchId,
                Points=0,
                UserId=user.UserId,
                VisitorGoals = int.Parse(GoalsVisitor),
            };

           
            var response = await _apiService.Post(parameters.UrlBase, "/api", "/Predictions",
                user.TokenType,
                user.AccessToken,prediction);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Ha ocurrido un error al momento de guardar la prediccion, intente luego");
                return;
            }
            await _navigationService.Back();
            //if (LocalGoals2 == null)
            //{
            //    await dialogService.ShowMessage("Error","Necesitas ingresar una prediccion para el Local");
            //    return;
            //}
        }
        #endregion
    }

}
