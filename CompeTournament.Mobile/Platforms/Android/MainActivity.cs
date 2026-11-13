using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using AndroidX.Core.App;
using CompeTournament.Mobile.Core.Services;
using Firebase.Messaging;
using Microsoft.Maui.Storage;

namespace CompeTournament.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "copaamistosa", DataHost = "join")]
    public class MainActivity : MauiAppCompatActivity, IOnCompleteListener
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleAppLink(Intent);
            RequestNotificationPermission();
            FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(this);
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (task.IsSuccessful && task.Result != null)
            {
                Preferences.Set("fcm_token", task.Result.ToString());
            }
        }

        private void RequestNotificationPermission()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33)
                && CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Android.Manifest.Permission.PostNotifications }, 0);
            }
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
