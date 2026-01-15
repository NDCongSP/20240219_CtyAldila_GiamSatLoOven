using GiamSat.Models;
using GiamSat.UI.Model;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace GiamSat.UI.Shared
{
    public partial class NavMenu
    {
        [Parameter]
        public bool sidebarExpanded { get; set; } = true;

        private List<OvenSystemModel> _ovenSystems = new List<OvenSystemModel>();
        private OvensInfo _ovensInfo = new OvensInfo();
        private string linkC;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var res = await _ft01Client.GetAllAsync();

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 2000 });
                    return;

                }

                var ft01 = res.Data.ToList();
                if (ft01 != null && ft01.Count > 0)
                {
                    _ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(ft01.FirstOrDefault().C001);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "error", Detail = ex.Message, Duration = 2000 });
                return;
            }
        }
        void OnChildClicked(MenuItemEventArgs args)
        {
            GlobalVariable.BreadCrumbData = null;
            GlobalVariable.BreadCrumbData = new List<BreadCrumbModel>();

            GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel()
            {
                Text = "Cấu hình Oven",
            });

            GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel()
            {
                Text = args.Text,
                Path = args.Path
            });
        }

        private void OnOvenClicked(OvenSystemModel oven, MenuItemEventArgs args)
        {
            // Update breadcrumb - chỉ hiển thị tên lò và "Trang chủ"
            UpdateBreadcrumb(oven.Name, "Trang chủ");
            
            // Force re-render
            StateHasChanged();
        }
        
        private void UpdateBreadcrumb(string ovenName, string? menuName)
        {
            GlobalVariable.BreadCrumbData = new List<BreadCrumbModel>();
            
            GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel
            {
                Text = ovenName,
                Path = null
            });

            if (!string.IsNullOrEmpty(menuName))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel
                {
                    Text = menuName,
                    Path = null
                });
            }
        }
    }
}
