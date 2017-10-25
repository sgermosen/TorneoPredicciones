using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class GroupItemViewModel : Group
    {
        private NavigationService navigationService;



        public GroupItemViewModel()
        {
            navigationService = new NavigationService(); 
            
        }
        public ICommand SelectGroupCommand { get { return new RelayCommand(SelectGroup); } }

        private async  void SelectGroup()
        {
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.Positions=new PositionsViewModel(TournamentGroupId);
            await navigationService.Navigate("PositionsPage");
        }
    }
}
