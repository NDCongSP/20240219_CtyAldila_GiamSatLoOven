using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;

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

        RadzenDataGrid<ProfileModel> _profileGrid;
        IList<ProfileModel> _profile;

        async Task OpenProfile(int profileId)
        {
            var d = 1;
            //await DialogService.OpenAsync<DialogCardPage>($"Order {orderId}",
            //      new Dictionary<string, object>() { { "OrderID", orderId } },
            //      new DialogOptions() { Width = "700px", Height = "520px" });
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            try
            {
                _ovenId = int.TryParse(OvenId, out int value) ? value : 0;

                var res = await _ft01Client.GetAllAsync();

                if (res == null)
                    return;
                _ft01 = res.Data.ToList();

                _ovenInfo = JsonConvert.DeserializeObject<OvensInfo>(_ft01.FirstOrDefault().C001).FirstOrDefault(x => x.Id == _ovenId);
                _profile = _ovenInfo.Profiles;
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
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                _ovenId = int.TryParse(OvenId, out int value) ? value : 0;

                var res = await _ft01Client.GetAllAsync();

                if (res == null)
                    return;
                _ft01 = res.Data.ToList();

                _ovenInfo = JsonConvert.DeserializeObject<OvensInfo>(res.Data.FirstOrDefault().C001).FirstOrDefault(x => x.Id == _ovenId);
                _profile = _ovenInfo.Profiles;
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

                //lấy ra list tất cả các lò Oven
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(model.C001);

                #region cập nhật lại các thông số của oven được chọn để
                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == _ovenId);
                ovenUpdate.Name=arg.Name;
                ovenUpdate.Path=arg.Path;
                ovenUpdate.Profiles=arg.Profiles;
                #endregion

                model.C001 = JsonConvert.SerializeObject(ovensInfo);

                var res = await _ft01Client.UpdateAsync(model);

                if (!res.Succeeded)
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

        void ShowTooltip(ElementReference elementReference, TooltipOptions options = null)
        {
            _tooltipService.Open(elementReference, "Local Station/Channel/Device", options);
        }
    }
}
