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
 public   class SelectUserGroupPageViewModel:INotifyPropertyChanged
    {
        #region Attributes
        private ApiService apiService;
        private DataService dataService;
        private DialogService dialogService;
        private NavigationService navigationService;
        private bool isRefreshing = false;
        #endregion

        #region Properties
        public ObservableCollection<UserGroupItemViewModel> UserGroups { get; set; }

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
        public SelectUserGroupPageViewModel()
        {
           // instance = this;

            apiService = new ApiService();
            dialogService = new DialogService();
            navigationService = new NavigationService();
            dataService = new DataService();

            UserGroups = new ObservableCollection<UserGroupItemViewModel>();
            LoadUserGroups();
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
        private async void LoadUserGroups()
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
            var response = await apiService.Get<UserGroup>(parameter.URLBase, "/api", "/Groups", user.TokenType, user.AccessToken,user.UserId);
            if (!response.IsSuccess)
            {
                await dialogService.ShowMessage("Error", response.Message);
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
