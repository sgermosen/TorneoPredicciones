using CompeTournament.Mobile.Core;
using CompeTournament.Mobile.Views;

namespace CompeTournament.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(AppRoutes.Register, typeof(RegisterPage));
            Routing.RegisterRoute(AppRoutes.GroupDetail, typeof(GroupDetailPage));
            Routing.RegisterRoute(AppRoutes.MatchPrediction, typeof(MatchPredictionPage));
        }
    }
}
