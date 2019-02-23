using System;
using TorneoPredicciones.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TorneoPredicciones.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectMatchPage : ContentPage
    {
        public SelectMatchPage()
        {
            InitializeComponent();
            var selectMatchViewModel = SelectMatchViewModel.GetInstance();
            base.Appearing += (object sender, EventArgs e) =>
            {
                selectMatchViewModel.RefreshCommand.Execute(this);
            };
        }
    }
}