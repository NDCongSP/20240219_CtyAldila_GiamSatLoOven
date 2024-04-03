using Microsoft.AspNetCore.Components;
using Radzen;
using Toolbelt.Blazor;

namespace GiamSat.UI
{
    public class HttpInterceptorManager : IHttpInterceptorManager
    {
        private readonly NotificationService _notify;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClientInterceptor _httpInterceptor;
        private readonly JwtAuthenticationService _authService;

        public HttpInterceptorManager(NotificationService notify, NavigationManager navigationManager, HttpClientInterceptor httpInterceptor, JwtAuthenticationService authService)
        {
            _notify = notify;
            _navigationManager = navigationManager;
            _httpInterceptor = httpInterceptor;
            _authService = authService;
        }

        public void DisposeEvent()
        {
            _httpInterceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;
        }

        public async Task InterceptBeforeHttpAsync(object sender, Toolbelt.Blazor.HttpClientInterceptorEventArgs args)
        {
            var absPath = args.Request.RequestUri.AbsolutePath.ToLower();
            if (!absPath.Contains("login") && !absPath.Contains("refreshtoken"))
            {
                try
                {
                    var result = await _authService.TryRefreshToken();
                    if (result != null)
                    {
                        args.Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    _notify.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Your session was expired",
                        Duration = 2000
                    });
                    await _authService.LogoutAsync();
                }
            }
        }

        public void RegisterEvent()
        {
            _httpInterceptor.BeforeSendAsync += InterceptBeforeHttpAsync;
        }
    }
}
