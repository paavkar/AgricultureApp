namespace AgricultureApp.MauiClient.Data
{
    public static class Constants
    {
#if DEBUG
#if ANDROID
        public const string ApiBaseUrl = "http://10.0.2.2:5127/api/";
        public const string BaseUrl = "http://10.0.2.2:5127";
#elif WINDOWS
        public const string ApiBaseUrl = "https://localhost:7032/api/";
        public const string BaseUrl = "https://localhost:7032";
#else
        public const string ApiBaseUrl = "https://localhost:7032/api/";
        public const string BaseUrl = "https://localhost:7032";
#endif
#else
        public const string ApiBaseUrl = "https://<WHATEVER_THE_URL_IS>/api/";
        public const string BaseUrl = "https://<WHATEVER_THE_URL_IS>";
#endif
    }
}
