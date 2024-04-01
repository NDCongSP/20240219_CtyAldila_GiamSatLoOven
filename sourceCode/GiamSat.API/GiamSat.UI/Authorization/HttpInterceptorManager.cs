using Microsoft.AspNetCore.Components;
using Radzen;
using Toolbelt.Blazor;

namespace GiamSat.UI
{
    public class HttpInterceptorManager : IHttpInterceptorManager
    {
        private readonly NotificationService _snackBar;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClientInterceptor _httpInterceptor;
        private readonly JwtAuthenticationService _authService;

        public HttpInterceptorManager(NotificationService snackBar, NavigationManager navigationManager, HttpClientInterceptor httpInterceptor, JwtAuthenticationService authService)
        {
            _snackBar = snackBar;
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
                    //_snackBar.Add("Your session was expired", Severity.Error);
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
