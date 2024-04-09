
using Blazored.LocalStorage;
using DocumentFormat.OpenXml.InkML;
using GiamSat.Models;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Radzen;
using System.Security.Claims;

namespace GiamSat.UI.Shared
{
    public partial class MainLayout : IDisposable
    {
        [Inject] IHttpInterceptorManager _httpInterceptorManager { get; set; }

        bool _sidebarExpanded = false;
        bool _usersManager = false;
        bool _changePass = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await _authSerivce.GetAuthenticationStateAsync();
                if (authState.User.Identity == null || !authState.User.Identity.IsAuthenticated)
                {
                    return;
                }

                _httpInterceptorManager.RegisterEvent();

                #region đọc thông số để có cho phép user đổi pass hay ko
                var config = await _ft01Client.GetAllAsync();

                if (!config.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Kết nối APIP lỗi",
                        Duration = 4000
                    });
                    return;
                }

                var c = JsonConvert.DeserializeObject<ConfigModel>(config.Data.ToList().FirstOrDefault().C000);
                _changePass = c.ChangePassUser;
                #endregion

                
                GlobalVariable.UserName = authState.User.Identity.Name;

                //var t = authState.User.FindFirst("testabc").Value;
                //var email = authState.User.FindFirst("emailTest").Value;

                var claimRole = authState.User.FindAll(ClaimTypes.Role)?.ToList();

                foreach (var item in claimRole)
                {
                    if (item.Value == "Admin")
                    {
                        _usersManager = true;
                        _changePass = true;
                        return;
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
