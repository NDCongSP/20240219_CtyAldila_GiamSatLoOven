using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using GiamSat.Models;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Xml.Serialization;

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


        async Task OpenItem(int profileId)
        {
            var model = _ft01.FirstOrDefault();
            var res = await _dialogService.OpenAsync<DialogCardPageEditProfile>($"Chỉnh sửa profile ID: {profileId}",
                    new Dictionary<string, object>() { { "OvenId", _ovenId }, { "ProfileID", profileId } },
                    new DialogOptions() { Width = "1000px", Height = "750px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });

            if (res == "Success")
            {
                RefreshData();
            }
        }

        async Task DeleteItem(int profileId)
        {
            try
            {
                var model = _ft01.FirstOrDefault();
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(model.C001);
                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == _ovenId);
                var profile = ovenUpdate.Profiles.Where(x => x.Id == profileId).FirstOrDefault();

                var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn xóa profile {profile.Name}", "Xóa profile", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

                ovenUpdate.Profiles.Remove(profile);
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

        async Task AddNewItem(int ovenId)
        {
            try
            {
                var confirm = await _dialogService.Confirm("Bạn chắc chắn muốn thêm profile?", "Tạo mới profile", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

                var model = _ft01.FirstOrDefault();

                //lấy ra list tất cả các lò Oven
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(model.C001);

                #region cập nhật lại các thông số của oven được chọn để
                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == _ovenId);

                var maxIdProfile = ovenUpdate.Profiles.ToArray().Count();

                //var maxIdProfileCurrent = JsonConvert.DeserializeObject<ProfileModel>(ovenUpdate.Profiles).Max;
                ovenUpdate.Profiles.Add(new ProfileModel()
                {
                    Id = maxIdProfile + 1,
                    Name = $"Profile {maxIdProfile + 1}"
                });
                #endregion

                model.C001 = JsonConvert.SerializeObject(ovensInfo);

                var data = await _ft01Client.UpdateAsync(model);

                if (!data.Succeeded)
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
                    Summary = "Tạo profile",
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
                var confirm = await _dialogService.Confirm("Bạn chắc chắn muốn lưu thông tin?", "Lưu thông tin", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

                var model = _ft01.FirstOrDefault();

                //lấy ra list tất cả các lò Oven
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(model.C001);

                #region cập nhật lại các thông số của oven được chọn để
                var ovenUpdate = ovensInfo.FirstOrDefault(x => x.Id == _ovenId);
                ovenUpdate.Name = arg.Name;
                ovenUpdate.Path = arg.Path;
                ovenUpdate.Profiles = arg.Profiles;
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

        async void RefreshData()
        {
            try
            {
                _ovenId = int.TryParse(OvenId, out int value) ? value : 0;

                var res = await _ft01Client.GetAllAsync();

                if (res == null)
                    return;
                _ft01 = res.Data.ToList();

                _ovenInfo = JsonConvert.DeserializeObject<OvensInfo>(_ft01.FirstOrDefault().C001).FirstOrDefault(x => x.Id == _ovenId);
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

            StateHasChanged();
        }

        void ShowTooltip(ElementReference elementReference, TooltipOptions options = null)
        {
            _tooltipService.Open(elementReference, "Local Station/Channel/Device", options);
        }
    }
}
