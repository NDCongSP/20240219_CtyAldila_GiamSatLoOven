using DocumentFormat.OpenXml.Drawing.Charts;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace GiamSat.UI.Pages
{
    public partial class OvenConfig
    {
        [Parameter]
        public string OvenId { get; set; }

        Variant variant = Variant.Outlined;

        private List<APIClient.FT01> _ft01 = new List<APIClient.FT01>();
        private OvenInfoModel _ovenInfo = new OvenInfoModel();
        private int _ovenId = 0;


        protected override async Task OnInitializedAsync()
        {
            try
            {
                _ovenId = int.TryParse(OvenId, out int value) ? value : 0;

                var res = await _ft01Client.GetAllAsync();

                if (res == null)
                    return;
                _ft01 = res.Data.ToList();

                _ovenInfo = JsonConvert.DeserializeObject<OvensInfo>(res.Data.FirstOrDefault().C001).FirstOrDefault(x => x.Id == _ovenId);
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 4000
                });
                return;
            }
        }

        async void Submit(OvenInfoModel arg)
        {
            try
            {
                var model = _ft01.FirstOrDefault();
                model.C001 = JsonConvert.SerializeObject(arg);

                var res=await _ft01Client.UpdateAsync(model);

                if(!res.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Cập nhật thất bại.",
                        Duration = 4000
                    });

                    return;
                }

                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "Cập nhật thành công",
                    Duration = 4000
                });
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 4000
                });
                return;
            }
        }

        void Cancel()
        {
            var b = 10;
        }
    }
}
