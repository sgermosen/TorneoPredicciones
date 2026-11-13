using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CompeTournament.Mobile.Core.Services;

namespace CompeTournament.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "copaamistosa", DataHost = "join")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleAppLink(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleAppLink(intent);
        }

        private static void HandleAppLink(Intent? intent)
        {
            if (intent?.Action == Intent.ActionView && intent.Data != null)
            {
                var code = intent.Data.GetQueryParameter("code");
                if (!string.IsNullOrWhiteSpace(code))
                {
                    AppLinkState.PendingInviteCode = code;
                }
            }
        }
    }
}
