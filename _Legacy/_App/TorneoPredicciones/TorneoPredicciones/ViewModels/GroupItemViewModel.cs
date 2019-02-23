using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
    public class GroupItemViewModel : Group
    {
        private readonly NavigationService _navigationService;
        private readonly DataService _dataService;


        public GroupItemViewModel()
        {
            _navigationService = new NavigationService();
            _dataService = new DataService();
        }
        public ICommand SelectGroupCommand { get { return new RelayCommand(SelectGroup); } }

        private async  void SelectGroup()
        {

            var mainViewModel = MainViewModel.GetInstance();
            var parameters = _dataService.First<Parameter>(false);
            if (parameters.Option == "Tournaments")
            {
                mainViewModel.Positions = new PositionsViewModel(TournamentGroupId);
                await _navigationService.Navigate("PositionsPage");
            }
            else
            {
                mainViewModel.MyResults = new MyResultsViewModel(TournamentGroupId);
                await _navigationService.Navigate("MyResultsPage");
            }

            
           
        }
    }
}
