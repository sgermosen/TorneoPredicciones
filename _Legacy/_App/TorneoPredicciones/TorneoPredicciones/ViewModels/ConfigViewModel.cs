using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using TorneoPredicciones.Models;
using TorneoPredicciones.Classes;
using TorneoPredicciones.Services;
using Xamarin.Forms;

namespace TorneoPredicciones.ViewModels
{
    public class ConfigViewModel : User, INotifyPropertyChanged
    {

        #region Evento
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private readonly NavigationService _navigationService;
        private readonly DataService _dataService;
        private bool _isRunning;
        private bool _isEnabled;
        private int _favoriteLeagueId;
        private int _favoriteTeamId;
        private List<League> _leagues;
        private ImageSource _imageSource;
        private MediaFile _file;
        private readonly User _currentUser;
        private bool _allowToModify;
        #endregion

        #region Properties

        public ObservableCollection<LeagueItemViewModel> Leagues { get; set; }

        public ObservableCollection<TeamItemViewModel> Teams { get; set; }

        public ImageSource ImageSource
        {
            set {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageSource"));
                }
            }
            get {
                return _imageSource;
            }
        }

        public int FavoriteLeagueId
        {
            set {
                if (_favoriteLeagueId != value)
                {
                    _favoriteLeagueId = value;
                    RealoadTeams(_favoriteLeagueId);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FavoriteLeagueId"));
                }
            }
            get {
                return _favoriteLeagueId;
            }
        }
        public new int FavoriteTeamId
        {
            set {
                if (_favoriteTeamId != value)
                {
                    _favoriteTeamId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FavoriteTeamId"));
                }
            }
            get {
                return _favoriteTeamId;
            }
        }

        public bool AllowToModify
        {
            set {
                if (_allowToModify != value)
                {
                    _allowToModify = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AllowToModify"));
                }
            }
            get {
                return _allowToModify;
            }
        }

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

        #endregion

        #region Metodos
        private void RealoadTeams(int favoriteLeagueId)
        {
            var teams = _leagues.Where(l => l.LeagueId == favoriteLeagueId).FirstOrDefault().Teams;
            Teams.Clear();
            foreach (var team in teams.OrderBy(t => t.Name))
            {
                Teams.Add(new TeamItemViewModel
                {
                    Fans = team.Fans,
                    Initials = team.Initials,
                    LeagueId = team.LeagueId,
                    Logo = team.Logo,
                    Name = team.Name,
                    TeamId = team.TeamId,
                });
            }
        }

        private async void LoadLeagues()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("notasti.com");
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
            // var user = dataService.First<User>(false);
            var response = await _apiService.Get<League>(parameters.UrlBase, "/api", "/Leagues");

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                // IsRunning = false;
                // IsEnabled = true;
                await _dialogService.ShowMessage("Error", response.Message);
                return;
            }

            _leagues = (List<League>)response.Result;
            ReloadLeagues(_leagues);
            RealoadTeams(_currentUser.FavoriteTeam.LeagueId);
            _favoriteLeagueId = _currentUser.FavoriteTeam.LeagueId;
            FavoriteTeamId = _currentUser.FavoriteTeamId;
        }

        private void ReloadLeagues(List<League> leagues)
        {
            Leagues.Clear();
            foreach (var league in leagues.OrderBy(l => l.Name)) //pero mas mejor si lo mando desde el api
            {
                Leagues.Add(new LeagueItemViewModel
                {
                    LeagueId = league.LeagueId,
                    Logo = league.Logo,
                    Name = league.Name,
                    Teams = league.Teams,

                });

            }
        }

        #endregion

        #region Comandos

        public ICommand ChangePasswordCommand { get { return new RelayCommand(ChangePassword); } }

        private async void ChangePassword()
        {
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.ChangePassword = new ChangePasswordViewModel();
            await _navigationService.Navigate("ChangePasswordPage");
        }


        public ICommand SaveCommand { get { return new RelayCommand(Save); } }

        private async void Save()
        {
            if (string.IsNullOrEmpty(FirstName))
            {
                await _dialogService.ShowMessage("Error", "You must enter a first name.");
                return;
            }

            if (string.IsNullOrEmpty(LastName))
            {
                await _dialogService.ShowMessage("Error", "You must enter a last name.");
                return;
            }

            if (string.IsNullOrEmpty(Email))
            {
                await _dialogService.ShowMessage("Error", "You must enter a email.");
                return;
            }

            if (string.IsNullOrEmpty(NickName))
            {
                await _dialogService.ShowMessage("Error", "You must enter a nick name.");
                return;
            }

            if (FavoriteTeamId == 0)
            {
                await _dialogService.ShowMessage("Error", "You must select a favorite team.");
                return;
            }

            if (!CrossConnectivity.Current.IsConnected)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("notasti.com");
            if (!isReachable)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            byte[] imageArray = null;
            if (_file != null)
            {
                imageArray = FilesHelper.ReadFully(_file.GetStream());
                _file.Dispose();
            }


            var user = new User
            {
                Email = Email,
                FavoriteTeamId = FavoriteTeamId,
                FirstName = FirstName,
                ImageArray = imageArray,
                LastName = LastName,
                NickName = NickName,
                Password = Password,
                UserTypeId = 1,
                UserId = _currentUser.UserId
            };

            var parameters = _dataService.First<Parameter>(false);
            var response = await _apiService.Put(parameters.UrlBase, "/api", "/Users",
                _currentUser.TokenType, _currentUser.AccessToken, user);


            if (!response.IsSuccess)
            {
                await _dialogService.ShowMessage("Error", response.Message);
                return;
            }

            response = await _apiService.GetUserByEmail(parameters.UrlBase,
               "/api", "/Users/GetUserByEmail", _currentUser.TokenType, _currentUser.AccessToken, Email);

            var newUser = (User)response.Result;

            newUser.AccessToken = _currentUser.AccessToken;
            newUser.TokenType = _currentUser.TokenType;
            newUser.TokenExpires = _currentUser.TokenExpires;
            newUser.IsRemembered = _currentUser.IsRemembered;
            newUser.Password = _currentUser.Password;
            _dataService.DeleteAllAndInsert(newUser.FavoriteTeam);
            _dataService.DeleteAllAndInsert(newUser.UserType);
            _dataService.DeleteAllAndInsert(newUser);

            IsRunning = false;
            IsEnabled = true;

            var mainViewModel = MainViewModel.GetInstance();
            //currentUser.Email = Email;
            //currentUser.FavoriteTeamId = FavoriteTeamId;
            //currentUser.FirstName = FirstName;
            //currentUser.ImageArray = imageArray;
            //currentUser.LastName = LastName;
            //currentUser.NickName = NickName;
            //currentUser.Password = Password;
            mainViewModel.CurrentUser = newUser;
            await _navigationService.Back();

        }

        public ICommand CancelCommand { get { return new RelayCommand(Cancel); } }

        private void Cancel()
        {
            _navigationService.SetMainPage("LoginPage");
        }

        public ICommand TakePictureCommand { get { return new RelayCommand(TakePicture); } }

        private async void TakePicture()
        {
            if (!_allowToModify)
            {
                return;
            }
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await _dialogService.ShowMessage("No Camera", ":( No camera available.");
                return;
            }

            IsRunning = true;

            _file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg",
                PhotoSize = PhotoSize.Small,
            });

            if (_file != null)
            {
                ImageSource = ImageSource.FromStream(() =>
                {
                    var stream = _file.GetStream();
                    return stream;
                });
            }

            IsRunning = false;
        }

        #endregion

        #region Constructor
        public ConfigViewModel(User currentUser)
        {
            this._currentUser = currentUser;

            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService = new DataService();

            Leagues = new ObservableCollection<LeagueItemViewModel>();
            Teams = new ObservableCollection<TeamItemViewModel>();
            // IsRunning = false;
            FirstName = currentUser.FirstName;
            LastName = currentUser.LastName;
            Picture = currentUser.Picture;
            Email = currentUser.Email;// currentUser.Email.Substring(7);
            NickName = currentUser.NickName;

            IsEnabled = true;

            _allowToModify = currentUser.UserTypeId == 1;

            LoadLeagues();

        }


        #endregion

    }
}
