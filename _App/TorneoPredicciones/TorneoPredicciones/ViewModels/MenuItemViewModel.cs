using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Services;
using TorneoPredicciones.Models;

namespace TorneoPredicciones.ViewModels
{
    public class MenuItemViewModel
    {
        #region Atributos

        private readonly NavigationService _navigationService;
        private readonly DataService _dataService;

        #endregion

        #region Properties
        public string Icon { get; set; }

        public string Title { get; set; }

        public string PageName { get; set; }
        #endregion

        #region Commands
        public ICommand NavigateCommand { get { return new RelayCommand(Navigate); } }

        private async void Navigate()
        {var mainViewModel = MainViewModel.GetInstance();
            if (PageName == "LoginPage")
            {
                
                mainViewModel.CurrentUser.IsRemembered = false;
                _dataService.Update(mainViewModel.CurrentUser);
                _navigationService.SetMainPage("LoginPage");
            }
            else
            {
                switch (PageName)
                {//este señor manda a una pagina compartida, por lo que es prudente almacenar de donde fue llamado para saber a donde volver
                    case "SelectTournamentPage":
                        var parameters = _dataService.First<Parameter>(false);
                        parameters.Option = Title;//quien diferencia las dos opciones
                        _dataService.Update(parameters);
                        mainViewModel.SelectTournament=new SelectTournamentViewModel();
                        await _navigationService.Navigate(PageName);
                        break;
                    case "ConfigPage":
                        mainViewModel.Config = new ConfigViewModel(mainViewModel.CurrentUser);
                        await _navigationService.Navigate(PageName);
                        break;
                    case "SelectUserGroupPage":
                        mainViewModel.SelectUserGroup = new SelectUserGroupViewModel();
                        await _navigationService.Navigate(PageName);
                        break;

                        
                    default:
                      
                        break;
                        
                }
                //var parameters = dataService.First<Parameter>(false);
                //parameter.Option = Title;
                //dataService.Update(parameter);
               
            }
        }

        #endregion

        #region Cosntructores

        public MenuItemViewModel()
        {
            _navigationService = new NavigationService();
            _dataService= new DataService();
        }

        #endregion

    }
}
