using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
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
        public int OvenId { get; set; }
        [Parameter] public int ProfileId { get; set; }

        private APIClient.FT01 _ft01 = new APIClient.FT01();

        private ProfileModel _profileInfo = new ProfileModel();

        RadzenDataGrid<StepModel> _stepGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 5, 10, 20, 30 };
        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            RefreshData();
        }

        async void RefreshData()
        {
            try
            {
                var res = await _ft01Client.GetAllAsync();

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Không có dữu liệu",
                        Duration = 4000
                    });
                    return;
                }

                _ft01 = res.Data.ToList().FirstOrDefault();

                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(_ft01.C001);
                var ovenInfo = ovensInfo.FirstOrDefault(x => x.Id == OvenId);
                _profileInfo = ovenInfo.Profiles.FirstOrDefault(x => x.Id == ProfileId);
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 40000
                });
                return;
            }

            StateHasChanged();
        }

        async void Submit(ProfileModel arg)
        {
            try
            {
                var confirm = await _dialogService.Confirm("Bạn chắc chắn muốn lưu thông tin?", "Cập nhật profile", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

                //lấy ra list tất cả các lò Oven
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(_ft01.C001);

                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == OvenId);
                var profileInfo = ovenUpdate.Profiles.FirstOrDefault(x => x.Id == ProfileId);
                profileInfo.Name = arg.Name;
                profileInfo.Steps = arg.Steps;

                _ft01.C001 = JsonConvert.SerializeObject(ovensInfo);

                var res = await _ft01Client.UpdateAsync(_ft01);

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
                    Detail = "Cập nhật thành công.",
                    Duration = 4000
                });

                _dialogService.Close("Success");
                //RefreshData();
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

        async void AddNewItem(int profileId)
        {
            var res = await _dialogService.OpenAsync<DialogCardPageAddStep>($"Thêm bước chạy cho profile Id: {profileId}",
                    new Dictionary<string, object>() { { "Model", _ft01 }, { "OvenId", OvenId }, { "ProfileID", profileId }, { "StepId", 0 } },
                    new DialogOptions() { Width = "1200px", Height = "470px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });

            if (res == "Success")
            {
                RefreshData();
            }
        }
        async Task OpenItem(int profileId, int stepId)
        {
            var res = await _dialogService.OpenAsync<DialogCardPageAddStep>($"Sửa bước chạy Id: {stepId}",
                     new Dictionary<string, object>() { { "Model", _ft01 }, { "OvenId", OvenId }, { "ProfileID", profileId }, { "StepId", stepId } },
                     new DialogOptions() { Width = "1200px", Height = "470px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });

            if (res == "Success")
            {
                RefreshData();
            }
        }

        async Task DeleteItem(int stepId)
        {
            try
            {
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(_ft01.C001);
                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == OvenId);
                var profile = ovenUpdate.Profiles.Where(x => x.Id == _profileInfo.Id).FirstOrDefault();
                var step = profile.Steps.FirstOrDefault(x => x.Id == stepId);

                var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn xóa profile {profile.Name}", "Xóa profile"
                    , new ConfirmOptions()
                    {
                        OkButtonText = "Yes",
                        CancelButtonText = "No",
                        AutoFocusFirstElement = true,
                    });

                if (confirm == null || confirm == false) return;

                profile.Steps.Remove(step);
                _ft01.C001 = JsonConvert.SerializeObject(ovensInfo);
                var res = await _ft01Client.UpdateAsync(_ft01);

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
                    Detail = "Cập nhật thành công.",
                    Duration = 4000
                });

                RefreshData();
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
    }
}
