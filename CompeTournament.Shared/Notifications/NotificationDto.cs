namespace CompeTournament.Shared.Notifications
{
    using System;

    public class NotificationDto
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public bool IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
