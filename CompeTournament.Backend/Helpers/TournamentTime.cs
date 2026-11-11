namespace CompeTournament.Backend.Helpers
{
    using System;

    public static class TournamentTime
    {
        public static DateTime ToTournamentTime(this DateTime utc, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

        public static DateTime ToUtcFromTournamentTime(this DateTime local, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), zone);
    }
}
