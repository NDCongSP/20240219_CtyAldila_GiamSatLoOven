
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
        [Inject] NavigationManager _navigationManager { get; set; }

        bool _sidebarExpanded = false;
        bool _usersManager = false;
        bool _changePass = false;        

        protected override async Task OnInitializedAsync()
        {
            _navigationManager.LocationChanged += HandleLocationChanged;
            UpdateBreadcrumbFromUrl(_navigationManager.Uri);

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
                _changePass = true;
                
                var claimRole = authState.User.FindAll(ClaimTypes.Role)?.ToList();

                foreach (var item in claimRole)
                {
                    if (item.Value == "Admin")
                    {
                        _usersManager = true;
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

        async Task OnClickLogout()
        {
            await _authSerivce.LogoutAsync();
        }

        public void Dispose()
        {
            _httpInterceptorManager.DisposeEvent();
            _navigationManager.LocationChanged -= HandleLocationChanged;
        }

        private void HandleLocationChanged(object sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            UpdateBreadcrumbFromUrl(e.Location);
            StateHasChanged();
        }

        private void UpdateBreadcrumbFromUrl(string url)
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.ToLower();

            GlobalVariable.BreadCrumbData = new List<BreadCrumbModel>();

            if (path.Contains("/oven-home"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Hệ thống lò OVEN" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Trang chủ" });
            }
            else if (path.Contains("/ovenconfig"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Hệ thống lò OVEN" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Cấu hình" });
            }
            else if (path.Contains("/report") && !path.Contains("/revo") && !path.Contains("/temperature"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Hệ thống lò OVEN" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Báo cáo" });
            }
            else if (path.Contains("/settings"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Hệ thống lò OVEN" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Cài đặt" });
            }
            else if (path.Contains("/revo/config"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "REVO" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Cấu hình" });
            }
            else if (path.Contains("/revo/report"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "REVO" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Báo cáo" });
            }
            else if (path.Contains("/revo"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "REVO" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Trang chủ" });
            }
            else if (path.Contains("/temperature/dashboard"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Giám sát Nhiệt độ" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Trạm điều hành" });
            }
            else if (path.Contains("/temperature/config"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Giám sát Nhiệt độ" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Cấu hình" });
            }
            else if (path.Contains("/temperature/report"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Giám sát Nhiệt độ" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Cảnh báo sự cố" });
            }
            else if (path.Contains("/admin/users"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Quản trị hệ thống" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Tài khoản" });
            }
            else if (path.Contains("/admin/roles"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Quản trị hệ thống" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Quyền & Vai trò" });
            }
            else if (path.Contains("/admin/permissions"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Quản trị hệ thống" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "System Permissions" });
            }
            else if (path.Contains("/admin/logs"))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "Quản trị hệ thống" });
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel { Text = "System Logs" });
            }
        }
    }
}
