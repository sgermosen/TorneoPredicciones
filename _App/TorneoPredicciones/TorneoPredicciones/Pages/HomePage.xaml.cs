using System;
using TorneoPredicciones.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TorneoPredicciones.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        
        public HomePage()
        {
            InitializeComponent();

            var vm = MainViewModel.GetInstance();
            base.Appearing += (object sender, EventArgs e) =>
            {
                vm.RefreshPointsCommand.Execute(this);
            };
        
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var mainViewmodel = MainViewModel.GetInstance();

        }
    }
}