using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AgricultureApp.MauiClient.Utilities
{
    public partial class JwtAuthHandler(
        AuthenticationService authService,
        PlatformInfoService platformInfoService) : DelegatingHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await authService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var platform = platformInfoService.GetPlatformInfo();
            request.Headers.Add("X-Client-Platform", platform);

            var locale = await SecureStorage.GetAsync("preferred_culture");
            request.Headers.AcceptLanguage.Clear();
            request.Headers.AcceptLanguage.Add(
                new StringWithQualityHeaderValue(locale ?? "en-GB"));

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshed = await authService.RefreshTokensAsync();

                if (refreshed)
                {
                    var newAccessToken = await authService.GetAccessTokenAsync();

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", newAccessToken);

                    if (request.Content != null)
                    {
                        var content = await request.Content.ReadAsStringAsync();
                        request.Content = new StringContent(content, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType);
                    }

                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}
