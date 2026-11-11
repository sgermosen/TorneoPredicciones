namespace CompeTournament.Mobile.Services
{
    public static class ApiConstants
    {
#if ANDROID && DEBUG
        public const string BaseUrl = "https://10.0.2.2:5001/";
#elif DEBUG
        public const string BaseUrl = "https://localhost:5001/";
#else
        public const string BaseUrl = "https://competournament-demo-6tlxjme4gq-uc.a.run.app/";
#endif
    }
}
