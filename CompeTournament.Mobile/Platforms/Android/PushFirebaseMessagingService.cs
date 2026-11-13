using System;
using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Firebase.Messaging;
using Microsoft.Maui.Storage;

namespace CompeTournament.Mobile
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushFirebaseMessagingService : FirebaseMessagingService
    {
        private const string ChannelId = "copa_amistosa";

        public override void OnNewToken(string token)
        {
            Preferences.Set("fcm_token", token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            var notification = message.GetNotification();
            var title = notification?.Title ?? "Copa Amistosa";
            var body = notification?.Body ?? string.Empty;

            var manager = (NotificationManager)GetSystemService(NotificationService);
            if (manager == null)
            {
                return;
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                manager.CreateNotificationChannel(new NotificationChannel(ChannelId, "Copa Amistosa", NotificationImportance.Default));
            }

            var builder = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetSmallIcon(ApplicationInfo.Icon)
                .SetAutoCancel(true);

            manager.Notify(Environment.TickCount, builder.Build());
        }
    }
}
