namespace CompeTournament.Mobile.Services
{
    public static class ApiConstants
    {
#if ANDROID
        public const string BaseUrl = "https://10.0.2.2:5001/";
#else
        public const string BaseUrl = "https://localhost:5001/";
#endif
    }
}
