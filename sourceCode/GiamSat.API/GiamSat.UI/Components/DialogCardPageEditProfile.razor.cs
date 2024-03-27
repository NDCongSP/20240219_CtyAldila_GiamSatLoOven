using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using GiamSat.APIClient;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Linq.Dynamic.Core.Tokenizer;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageEditProfile
    {
        [Parameter]
        public APIClient.FT01 Model { get; set; }
        [Parameter]
        public int OvenId { get; set; }
        [Parameter] public int ProfileId { get; set; }
        
        
        private ProfileModel _profileInfo = new ProfileModel();

        RadzenDataGrid<StepModel> _stepGrid;


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            //order = dbContext.Orders.Where(o => o.OrderID == OrderID)
            //.Include("Customer")
            //                  .Include("Employee").FirstOrDefault();

            //orderDetails = dbContext.OrderDetails.Include("Order").ToList();

            var ovensInfo=JsonConvert.DeserializeObject<OvensInfo>(Model.C001);
            var ovenInfo=ovensInfo.FirstOrDefault(x=>x.Id==OvenId);
            _profileInfo = ovenInfo.Profiles.FirstOrDefault(x=>x.Id==ProfileId);
        }

        async void Submit(ProfileModel arg)
        {
            try
            {
                //lấy ra list tất cả các lò Oven
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(Model.C001);

                //#region cập nhật lại các thông số của oven được chọn để
                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == OvenId);
                var profiInfo=ovenUpdate.Profiles.FirstOrDefault(x=>x.Id == ProfileId);


                //    ovenUpdate.Name = arg.Name;
                //    ovenUpdate.Path = arg.Path;
                //    ovenUpdate.Profiles = arg.Profiles;
                //    #endregion

                //    Model.C001 = JsonConvert.SerializeObject(ovensInfo);

                //    var res = await _ft01Client.UpdateAsync(Model);

                //    if (!res.Succeeded)
                //    {
                //        _notificationService.Notify(new NotificationMessage()
                //        {
                //            Severity = NotificationSeverity.Error,
                //            Summary = "Error",
                //            Detail = "Cập nhật thất bại.",
                //            Duration = 4000
                //        });

                //        return;
                //    }

                //    _notificationService.Notify(new NotificationMessage()
                //    {
                //        Severity = NotificationSeverity.Success,
                //        Summary = "Success",
                //        Detail = "Cập nhật thành công.",
                //        Duration = 4000
                //    });
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

        async    void AddNewItem(int profileId)
        {

        }
        async Task OpenItem(int profileId)
        {
            //var model = _ft01.FirstOrDefault();
            //await _dialogService.OpenAsync<DialogCardPageEditProfile>($"Chỉnh sửa profile: {profileId}",
            //      new Dictionary<string, object>() { { "Model", model }, { "OvenId", _ovenId }, { "ProfileID", profileId } },
            //      new DialogOptions() { Width = "700px", Height = "520px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });
        }

        async Task DeleteItem(int profileId)
        {
            //try
            //{
            //    var model = _ft01.FirstOrDefault();
            //    var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(model.C001);
            //    var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == _ovenId);
            //    var profile = ovenUpdate.Profiles.Where(x => x.Id == profileId).FirstOrDefault();

            //    var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn xóa profile {profile.Name}", "Xóa profile", new ConfirmOptions()
            //    {
            //        OkButtonText = "Yes",
            //        CancelButtonText = "No",
            //    });

            //    if (confirm == false) return;

            //    ovenUpdate.Profiles.Remove(profile);
            //    model.C001 = JsonConvert.SerializeObject(ovensInfo);
            //    var res = await _ft01Client.UpdateAsync(model);
            //    if (!res.Succeeded)
            //    {
            //        _notificationService.Notify(new NotificationMessage()
            //        {
            //            Severity = NotificationSeverity.Error,
            //            Summary = "Error",
            //            Detail = "Cập nhật thất bại.",
            //            Duration = 4000
            //        });
            //        return;
            //    }

            //    _notificationService.Notify(new NotificationMessage()
            //    {
            //        Severity = NotificationSeverity.Success,
            //        Summary = "Success",
            //        Detail = "Cập nhật thành công.",
            //        Duration = 4000
            //    });

            //    RefreshData();
            //}
            //catch (Exception ex)
            //{
            //    _notificationService.Notify(new NotificationMessage()
            //    {
            //        Severity = NotificationSeverity.Error,
            //        Summary = "Error",
            //        Detail = ex.Message,
            //        Duration = 4000
            //    });

            //    return;
            //}
        }
    }
}
