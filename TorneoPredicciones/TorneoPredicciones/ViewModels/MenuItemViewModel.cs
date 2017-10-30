using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Services;
using TorneoPredicciones.Models;

namespace TorneoPredicciones.ViewModels
{
    public class MenuItemViewModel
    {
        #region Atributos

        private NavigationService navigationService;
        private DataService dataService;

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
                dataService.Update(mainViewModel.CurrentUser);
                navigationService.SetMainPage("LoginPage");
            }
            else
            {
                switch (PageName)
                {//este señor manda a una pagina compartida, por lo que es prudente almacenar de donde fue llamado para saber a donde volver
                    case "SelectTournamentPage":
                        var parameters = dataService.First<Parameter>(false);
                        parameters.Option = Title;//quien diferencia las dos opciones
                        dataService.Update(parameters);
                        mainViewModel.SelectTournament=new SelectTournamentViewModel();
                        await navigationService.Navigate(PageName);
                        break;
                    case "ConfigPage":
                        mainViewModel.Config = new ConfigViewModel(mainViewModel.CurrentUser);
                        await navigationService.Navigate(PageName);
                        break;
                    case "SelectUserGroupPage":
                        mainViewModel.SelectUserGroupPage = new SelectUserGroupPageViewModel();
                        await navigationService.Navigate(PageName);
                        break;

                        
                    default:
                      
                        break;
                        
                }
                //var parameter = dataService.First<Parameter>(false);
                //parameter.Option = Title;
                //dataService.Update(parameter);
               
            }
        }

        #endregion

        #region Cosntructores

        public MenuItemViewModel()
        {
            navigationService = new NavigationService();
            dataService= new DataService();
        }

        #endregion

    }
}
