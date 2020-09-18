using GalaSoft.MvvmLight.Command;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using TorneoPredicciones.Classes;

namespace TorneoPredicciones.ViewModels
{
    //el primer parametro es de donde hereda, todos los demas son implementaciones
    public class NewUserViewModel : User, INotifyPropertyChanged //una clase solo puede heredar de una clase, pero puede implementar n interfaces
    {

        #region Evento
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        //private ApiService apiService;
        //private DialogService dialogService;
        //private DataService dataService;
        //private NavigationService navigationService;
        //private string email;
        //private string password;
        //private bool isRunning;
        //private bool isEnabled;
        //private List<League> leagues;
        //private int favoriteLeagueId;
        //private ImageSource imageSource;
        //private MediaFile file;
        private readonly ApiService _apiService;
        private readonly DialogService _dialogService;
        private readonly NavigationService _navigationService;
        private readonly DataService _dataService;
        private bool _isRunning;
        private bool _isEnabled;
        private int _favoriteLeagueId;
        private List<League> _leagues;
        private ImageSource _imageSource;
        private MediaFile _file;
        //private string PasswordConfirm;



        #endregion

        #region Properties
        public string PasswordConfirm { get; set; }
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

        #region Constructores
        public NewUserViewModel()
        {
            _apiService = new ApiService();
            _dialogService = new DialogService();
            _navigationService = new NavigationService();
            _dataService = new DataService();

            Leagues = new ObservableCollection<LeagueItemViewModel>();
            Teams = new ObservableCollection<TeamItemViewModel>();
            // IsRunning = false;

            Picture = "avatar_user.png";
            IsEnabled = true;

            LoadLeagues();

        }



        #endregion

        #region Metodos
        private void RealoadTeams(int favoriteLeagueId)
        {
            var teams = _leagues.Where(l => l.LeagueId == favoriteLeagueId).FirstOrDefault().Teams;
            Teams.Clear();
            foreach (var team in teams.OrderBy(t=>t.Name))
            {
                Teams.Add( new TeamItemViewModel
                {
                  Fans=team.Fans ,
                   Initials= team.Initials,
                   LeagueId= team.LeagueId,
                   Logo = team.Logo,
                   Name= team.Name,
                   TeamId= team.TeamId,
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

            if (string.IsNullOrEmpty(Password))
            {
                await _dialogService.ShowMessage("Error", "You must enter a password.");
                return;
            }

            if (Password.Length < 6)
            {
                await _dialogService.ShowMessage("Error", "The password must have at least 6 characters.");
                return;
            }

            if (string.IsNullOrEmpty(PasswordConfirm))
            {
                await _dialogService.ShowMessage("Error", "You must enter a password confirm.");
                return;
            }

            if (Password != PasswordConfirm)
            {
                await _dialogService.ShowMessage("Error", "The password and confirm does not match.");
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

            var isReachable = await CrossConnectivity.Current.IsRemoteReachable("praysoft.net");
            if (!isReachable)
            {
                await _dialogService.ShowMessage("Error", "Check you internet connection.");
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            var imageArray = FilesHelper.ReadFully(_file.GetStream());
            _file.Dispose();

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
            };

            var parameters = _dataService.First<Parameter>(false);
          //  var response = await _apiService.Post(parameters.UrlBase2, "/api", "/Users", user);
            var response = await _apiService.Post(parameters.UrlBase, "/api", "/Users", user);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                await _dialogService.ShowMessage("Error", response.Message);
                return;
            }

            await _dialogService.ShowMessage("Confirmation", "The user was created, please login.");
              _navigationService.SetMainPage("LoginPage");

        }

        public ICommand CancelCommand { get { return new RelayCommand(Cancel);} }

        private void Cancel()
        {
          _navigationService.SetMainPage("LoginPage");
        }

        public ICommand TakePictureCommand { get { return new RelayCommand(TakePicture); } }

        private async void TakePicture()
        {
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

        //public ICommand TakePictureCommand { get { return new RelayCommand(TakePicture);} }

        //private async void TakePicture()
        //{
        //    await CrossMedia.Current.Initialize();

        //    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
        //    {
        //        await dialogService.ShowMessage("No Camera", ":( No camera available.");
        //        return;
        //    }

        //    IsRunning = true;

        //    file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
        //    {
        //        Directory = "Sample",
        //        Name = "test.jpg",
        //        PhotoSize = PhotoSize.Small,
        //    });

        //    if (file != null)
        //    {
        //        ImageSource = ImageSource.FromStream(() =>
        //        {
        //            var stream = file.GetStream();
        //            return stream;
        //        });
        //    }

        //    IsRunning = false;
        //}

        #endregion

    }

}
