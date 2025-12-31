namespace AgricultureApp.MauiClient.Services
{
    public class PlatformInfoService
    {
        public string GetPlatformInfo()
        {
            var platform = DeviceInfo.Platform.ToString();   // Android, iOS, MacCatalyst, WinUI
            var version = DeviceInfo.VersionString;          // e.g. "10", "14.1", "22631"

            return $"{platform} - {version}";
        }
    }
}
