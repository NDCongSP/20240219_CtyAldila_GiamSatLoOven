using Microsoft.AspNetCore.Components;
using MudBlazor;
using Toolbelt.Blazor;

namespace GiamSat.UI.Authorization
{
    public class HttpInterceptorManager : IHttpInterceptorManager
    {
        private readonly ISnackbar _snackBar;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClientInterceptor _httpInterceptor;
        private readonly JwtAuthenticationService _authService;

        public HttpInterceptorManager(ISnackbar snackBar, NavigationManager navigationManager, HttpClientInterceptor httpInterceptor, JwtAuthenticationService authService)
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
            var absPath = args.Request.RequestUri.AbsolutePath;
            if (!absPath.Contains("login"))
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
                    _snackBar.Add("Your session was expired", Severity.Error);
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
