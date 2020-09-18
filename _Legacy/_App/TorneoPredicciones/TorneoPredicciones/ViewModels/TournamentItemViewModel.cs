using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
  public  class TournamentItemViewModel:Tournament
    {//todos los comandos que estan metidos dentro de una grilla deben ir en los itemsviewmodel
        private readonly NavigationService _navigationService;
        private readonly DataService _dataService;

        public TournamentItemViewModel()
        {
            _navigationService= new NavigationService();
            _dataService = new DataService();
        }

        public ICommand SelectTournamentCommand { get { return new RelayCommand(SelectTournament);} }

        private async void SelectTournament()
        {
            var mainViewModel = MainViewModel.GetInstance();
            var parameters = _dataService.First<Parameter>(false);
            if (parameters.Option == "Predictions")
            {
                mainViewModel.SelectMatch = new SelectMatchViewModel(TournamentId);
                await _navigationService.Navigate("SelectMatchPage");
            }
            else if (parameters.Option == "Predictions")
            {
                mainViewModel.SelectMatch = new SelectMatchViewModel(TournamentId);
                await _navigationService.Navigate("SelectMatchPage");
            }
            else
            {
               mainViewModel.SelectGroup = new SelectGroupViewModel(Groups);
                await _navigationService.Navigate("SelectGroupPage");
            }
          
        }
    }
}
