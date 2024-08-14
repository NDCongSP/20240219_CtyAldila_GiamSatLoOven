using GiamSat.APIClient;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using System;

namespace GiamSat.UI.Shared
{
    public partial class NavMenu
    {
        [Parameter]
        public bool sidebarExpanded { get; set; } = true;

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

        void OnParentClicked(MenuItemEventArgs args)
        {
            GlobalVariable.BreadCrumbData = null;
            GlobalVariable.BreadCrumbData = new List<BreadCrumbModel>();
            GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel() { 
            Text = args.Text,
            Path= args.Path
            });
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
    }
}
