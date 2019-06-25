using System.Collections.Generic;
using System.Collections.ObjectModel;
using TorneoPredicciones.Models;
//using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class SelectGroupViewModel
    {
        #region Attributes
        //private ApiService _apiService;
        //private DataService _dataService;
        //private DialogService _dialogService;
        //private NavigationService _navigationService;
        private readonly List<Group> _groups;
        #endregion

        #region Propiedades
        public ObservableCollection <GroupItemViewModel> Groups { get; set; }
        #endregion

        #region Constructor
        public SelectGroupViewModel(List<Group> groups)
        {

            //_apiService = new ApiService();
            //_dialogService = new DialogService();
            //_navigationService = new NavigationService();
            //_dataService = new DataService();

            this._groups = groups;
            Groups = new ObservableCollection<GroupItemViewModel>();
            // ReloadGroups(groups);
            LoadGroups();

        }
        //public SelectGroupViewModel(List<Group> groups, List<Group> groups1)
        //{
        //    //_apiService = new ApiService();
        //    //_dialogService = new DialogService();
        //    //_navigationService = new NavigationService();
        //    //_dataService = new DataService();
            
        //    this._groups = groups;
        //    Groups=new ObservableCollection<GroupItemViewModel>();
        //    // ReloadGroups(groups);
        //    LoadGroups();
        //}



        #endregion

        #region Metodos
        private void LoadGroups()
        {
            Groups.Clear();
            foreach (var group in _groups)
            {
                Groups.Add(new GroupItemViewModel
                {
                    Name = group.Name,
                    TournamentGroupId = group.TournamentGroupId,
                    TournamentId = group.TournamentId,
                });
            }
        }
    

        private void ReloadGroups(List<Group> list)
        {
           Groups.Clear();
            foreach (var group in _groups)
            {
                Groups.Add(new GroupItemViewModel
                {
                  Name=group.Name,
                   TournamentGroupId=group.TournamentGroupId,
                   TournamentId= group.TournamentId, 
                });
                
            }
        }


        #endregion



    }
}
