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
 public   class SelectUserGroupViewModel:INotifyPropertyChanged
    {
        #region Attributes
        private readonly ApiService _apiService;
        private readonly DataService _dataService;
        private readonly DialogService _dialogService;
        private NavigationService navigationService;
        private bool _isRefreshing;
        #endregion

        #region Properties
        public ObservableCollection<UserGroupItemViewModel> UserGroups { get; set; }

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
        public SelectUserGroupViewModel()
        {
           // instance = this;

            _apiService = new ApiService();
            _dialogService = new DialogService();
            navigationService = new NavigationService();
            _dataService = new DataService();

            UserGroups = new ObservableCollection<UserGroupItemViewModel>();
            LoadUserGroups();
        }
        #endregion

        #region Singleton
        private static SelectTournamentViewModel _instance;

        public static SelectTournamentViewModel GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SelectTournamentViewModel();
            }

            return _instance;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private async void LoadUserGroups()
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
            var parameter = _dataService.First<Parameter>(false);
            var user = _dataService.First<User>(false);
            var response = await _apiService.Get<UserGroup>(parameter.UrlBase, "/api", "/Groups", user.TokenType, user.AccessToken,user.UserId);
            if (!response.IsSuccess)
            {
                await _dialogService.ShowMessage("Error", response.Message);
                // await navigationService.Clear();
                return;
            }

            ReloadUserGroups((List<UserGroup>)response.Result);
        }

        private void ReloadUserGroups(List<UserGroup> userGroups)
        {
            UserGroups.Clear();
            foreach (var userGroup in userGroups)
            {
                UserGroups.Add(new UserGroupItemViewModel
                {
                   GroupId = userGroup.GroupId,
                    GroupUsers= userGroup.GroupUsers,
                    Logo= userGroup.Logo,
                    Owner= userGroup.Owner,
                    OwnerId= userGroup.OwnerId,
                    Name = userGroup.Name,
                    
                });
            }
        }
        #endregion

        #region Commands
        public ICommand RefreshCommand { get { return new RelayCommand(Refresh); } }

        public void Refresh()
        {
            //  IsRefreshing = true;
            LoadUserGroups();
            //  IsRefreshing = false;
        }
        #endregion
    }
}
