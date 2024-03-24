using DocumentFormat.OpenXml.Drawing.Charts;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace GiamSat.UI.Pages
{
    public partial class Settings
    {
        [Parameter]
        public string OvenId { get; set; }

        Variant variant = Variant.Outlined;

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

        void Submit(OvenInfoModel arg)
        {
            var d = 10;
        }

        void Cancel()
        {
            var b = 10;
        }
    }
}
