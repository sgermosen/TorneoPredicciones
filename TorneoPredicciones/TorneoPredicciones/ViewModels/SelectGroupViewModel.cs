using System.Collections.Generic;
using System.Collections.ObjectModel;
using TorneoPredicciones.Models;

namespace TorneoPredicciones.ViewModels
{
    public class SelectGroupViewModel
    {
        #region Atributos
        private List<Group> groups;


        #endregion
        #region Propiedades

       
        public ObservableCollection <GroupItemViewModel> Groups { get; set; }
        

        #endregion

        #region Constructor
        public SelectGroupViewModel(List<Group> groups)
        {
            this.groups = groups;
            Groups=new ObservableCollection<GroupItemViewModel>();
            ReloadGroups(groups);
        }



        #endregion

        #region Metodos
        private void ReloadGroups(List<Group> list)
        {
           Groups.Clear();
            foreach (var group in groups)
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
