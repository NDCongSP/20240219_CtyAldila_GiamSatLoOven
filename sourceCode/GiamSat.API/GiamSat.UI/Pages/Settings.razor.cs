using DocumentFormat.OpenXml.Drawing.Charts;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace GiamSat.UI.Pages
{
    public partial class Settings
    {

        Variant variant = Variant.Outlined;

        private List<APIClient.FT01> _ft01 = new List<GiamSat.APIClient.FT01>();
        private ConfigModel _configInfo = new ConfigModel();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authState = await _authSerivce.GetAuthenticationStateAsync();
                var n = authState.User.IsInRole("Admin");

                var res = await _ft01Client.GetAllAsync();

                if (!res.Succeeded)
                    return;

                _ft01 = res.Data.ToList();
                _configInfo = JsonConvert.DeserializeObject<ConfigModel>(_ft01.FirstOrDefault().C000);
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 2000
                });
                return;
            }
        }

        async Task Submit(ConfigModel arg)
        {
            try
            {
                var modelUpdate = _ft01.FirstOrDefault();
                modelUpdate.C000 = JsonConvert.SerializeObject(arg);

                var res = await _ft01Client.UpdateAsync(modelUpdate);

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Cập nhật thất bại.",
                        Duration = 2000
                    });

                    return;
                }

                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "Cập nhật thành công.",
                    Duration = 2000
                });
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 2000
                });
                return;
            }
        }

        void Cancel()
        {
            
        }
    }
}
