
using Blazored.LocalStorage;
using DocumentFormat.OpenXml.InkML;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Security.Claims;

namespace GiamSat.UI.Shared
{
    public partial class MainLayout : IDisposable
    {
        [Inject] IHttpInterceptorManager _httpInterceptorManager { get; set; }

        bool _sidebarExpanded = false;
        List<string> _role;
        bool _usersManager = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _httpInterceptorManager.RegisterEvent();

                var authState = await _authSerivce.GetAuthenticationStateAsync();
                GlobalVariable.UserName = authState.User.Identity.Name;

                var t = authState.User.FindFirst("testabc").Value;
                var email = authState.User.FindFirst("emailTest").Value;

                var claimRole = authState.User.FindAll(ClaimTypes.Role)?.ToList();

                foreach (var item in claimRole)
                {
                    _role.Add(item.Value);

                    if (item.Value == "Admin")
                    {
                        _usersManager = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
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
