using System.Collections.Generic;
using System.Collections.ObjectModel;
using TorneoPredicciones.Models;

namespace TorneoPredicciones.ViewModels
{
    public class UsersGroupViewModel : UserGroup
    {
        #region Atributos
        private UserGroup _userGroup;
        #endregion

        #region Propiedades
        public ObservableCollection<GroupUserItemViewModel> MyGroupUsers { get; set; }
        #endregion

        #region Contructor  
        public UsersGroupViewModel(UserGroup userGroup)
        {
            this._userGroup = userGroup;

            GroupId = userGroup.GroupId;
            Name = userGroup.Name;
            OwnerId = userGroup.OwnerId;
            Owner = userGroup.Owner;
            GroupUsers = userGroup.GroupUsers;

            MyGroupUsers = new ObservableCollection<GroupUserItemViewModel>();

            ReloadGroupUsers(GroupUsers);
        }



        #endregion

        #region Metodos
        private void ReloadGroupUsers(List<GroupUser> groupUsers)
        {
            MyGroupUsers.Clear();

            foreach (var groupUser in groupUsers)
            {
                MyGroupUsers.Add(new GroupUserItemViewModel
                {
                    GroupId = groupUser.GroupId,
                    GroupUserId = groupUser.GroupUserId,
                    IsAccepted = groupUser.IsAccepted,
                    IsBlocked = groupUser.IsBlocked,
                    Points = groupUser.Points,
                    User = groupUser.User,
                    UserId = groupUser.UserId,

                });


            }
        }


        #endregion


    }
}
