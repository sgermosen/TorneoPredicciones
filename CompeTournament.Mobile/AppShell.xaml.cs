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
            Routing.RegisterRoute(AppRoutes.CreateGroup, typeof(CreateGroupPage));
            Routing.RegisterRoute(AppRoutes.GroupDetail, typeof(GroupDetailPage));
            Routing.RegisterRoute(AppRoutes.GroupManage, typeof(GroupManagePage));
            Routing.RegisterRoute(AppRoutes.Redeem, typeof(RedeemPage));
            Routing.RegisterRoute(AppRoutes.CreditHistory, typeof(CreditHistoryPage));
            Routing.RegisterRoute(AppRoutes.Notifications, typeof(NotificationsPage));
            Routing.RegisterRoute(AppRoutes.Duels, typeof(DuelsPage));
            Routing.RegisterRoute(AppRoutes.MatchPrediction, typeof(MatchPredictionPage));
        }
    }
}
