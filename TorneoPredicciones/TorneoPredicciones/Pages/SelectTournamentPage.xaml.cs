using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoPredicciones.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TorneoPredicciones.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectTournamentPage : ContentPage
    {
        public SelectTournamentPage()
        {
            InitializeComponent();

            var instance = SelectTournamentViewModel.GetInstance();
            Appearing += (object sender, EventArgs e) =>
            {
                instance.RefreshCommand.Execute(this);
            };
        }
    }

}