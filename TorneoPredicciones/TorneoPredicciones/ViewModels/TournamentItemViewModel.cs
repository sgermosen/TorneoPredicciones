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
  public  class TournamentItemViewModel:Tournament
    {//todos los comandos que estan metidos dentro de una grilla deben ir en los itemsviewmodel
        private NavigationService navigationService;


        public TournamentItemViewModel()
        {
            navigationService= new NavigationService();
        }

        public ICommand SelectTournamentCommand { get { return new RelayCommand(SelectTournament);} }

        private async void SelectTournament()
        {
            var mainViewModel = MainViewModel.GetInstance();
            mainViewModel.SelectMatch= new SelectMatchViewModel(TournamentId);
            await navigationService.Navigate("SelectMatchPage");
        }
    }
}
