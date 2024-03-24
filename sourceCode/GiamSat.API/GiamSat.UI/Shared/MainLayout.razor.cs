
using Blazored.LocalStorage;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GiamSat.UI.Shared
{
    public partial class MainLayout
    {
        bool _sidebarExpanded = true;

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authSerivce.GetAuthenticationStateAsync();
            GlobalVariable.UserName = authState.User.Identity.Name;
        }

        async void OnclickLogout(MouseEventArgs args)
        {
            await _authSerivce.LogoutAsync();
        }
    }
}
