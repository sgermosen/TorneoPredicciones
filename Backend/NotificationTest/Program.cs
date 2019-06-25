using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;


namespace NotificationTest
{
    class Program
    {
        private static NotificationHubClient _hub;
        static void Main(string[] args)
        {
            _hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://psmhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=VRZxkd15WUJsWXWmkt0UySRsCg6K/9ZSytBIdsV7Grs=", "MainNotification");

            do
            {
                Console.WriteLine("Type a new message:");
                var message = Console.ReadLine();
                SendNotificationAsync(message);
                Console.WriteLine("The message was sent...");
            } while (true);

        }
        private static async void SendNotificationAsync(string message)
        {
            var tags = new List<string>();
            tags.Add("userId:1");
            tags.Add("userId:2");
            tags.Add("userId:3");
            tags.Add("userId:4");
            //a todo el mundo
            // await hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}");
            //a los que cumplen el tag
            await _hub.SendGcmNativeNotificationAsync("{ \"data\" : {\"Message\":\"" + message + "\"}}",tags);
        }

    }
}
