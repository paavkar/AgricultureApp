namespace AgricultureApp.MauiClient.Data
{
    public static class Constants
    {
#if DEBUG
#if ANDROID
        public const string ApiBaseUrl = "http://10.0.2.2:5127/api/";
#elif WINDOWS
        public const string ApiBaseUrl = "https://localhost:7032/api/";
#else
        public const string ApiBaseUrl = "https://localhost:7032/api/";
#endif
#else
        public const string ApiBaseUrl = "https://<WHATEVER_THE_URL_IS/api/";
#endif
    }
}
