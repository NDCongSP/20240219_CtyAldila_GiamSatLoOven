
using Blazored.LocalStorage;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GiamSat.UI.Shared
{
    public partial class MainLayout:IDisposable
    {
        [Inject] IHttpInterceptorManager _httpInterceptorManager { get; set; }

        bool _sidebarExpanded = false;

        protected override async Task OnInitializedAsync()
        {
            _httpInterceptorManager.RegisterEvent();

            var authState = await _authSerivce.GetAuthenticationStateAsync();
            GlobalVariable.UserName = authState.User.Identity.Name;

            var t = authState.User.FindFirst("testabc");
        }

        async void OnclickLogout(MouseEventArgs args)
        {
            await _authSerivce.LogoutAsync();
        }

        public void Dispose()
        {
            _httpInterceptorManager.DisposeEvent();
        }
    }
}
