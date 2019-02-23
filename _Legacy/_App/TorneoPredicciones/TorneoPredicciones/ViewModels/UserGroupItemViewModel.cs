using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
   public  class UserGroupItemViewModel:UserGroup
   {
       private readonly NavigationService navigationService;
        public ICommand SelectGroupCommand { get {return new RelayCommand(SelectGroup);} }

       public UserGroupItemViewModel()
       {
           navigationService = new NavigationService();

       }
        private async  void SelectGroup()
        {
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.UsersGroup = new UsersGroupViewModel(this);
           await navigationService.Navigate("UsersGroupPage");
        }
    }
}
