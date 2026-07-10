using Microsoft.UI.Xaml;

namespace CompeTournament.Mobile.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
