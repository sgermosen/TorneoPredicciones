using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TorneoPredicciones.Models;
using TorneoPredicciones.Services;

namespace TorneoPredicciones.ViewModels
{
  public  class MatchItemViewModel:Match
  {
        #region Atributos
      private NavigationService navigationService;


        #endregion

        #region Constructor

      public MatchItemViewModel()
      {
          navigationService = new NavigationService();
      }

        #endregion

        #region Comando

      public ICommand SelectMatchCommand
      {
          get { return new RelayCommand(SelectMatch); }
      }



        #endregion

        #region Metodos
      private async void SelectMatch()
      {
          var mainViewModel = MainViewModel.GetInstance();
          mainViewModel.EditPrediction = new EditPredictionViewModel(this);
          await navigationService.Navigate("EditPredictionPage");
      }


        #endregion

        





    }
}
