using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using System.Runtime.InteropServices;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageAddStep
    {
        [Parameter]
        public APIClient.FT01 Model { get; set; }
        [Parameter]
        public int OvenId { get; set; }
        [Parameter]
        public int ProfileId { get; set; }
        [Parameter]
        public int StepId { get; set; }

        StepModel _stepInfo = new StepModel();

        APIClient.FT01 _ft01 = new APIClient.FT01();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            RefreshData();
        }

        async void RefreshData()
        {
            try
            {
                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(Model.C001);
                var ovenInfo = ovensInfo.FirstOrDefault(x => x.Id == OvenId);
                var profileInfo = ovenInfo.Profiles.FirstOrDefault(x => x.Id == ProfileId);

                //StepId > 0 laf chỉnh sửa, =0 là tạo mới bước.
                if (StepId > 0)
                    _stepInfo = profileInfo.Steps.FirstOrDefault(x => x.Id == StepId);
                else
                {
                    try
                    {
                        _stepInfo.Id = profileInfo.Steps.Max(x => x.Id) + 1;
                    }
                    catch
                    {
                        _stepInfo.Id =  1;
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 4000
                });
                return;
            }
        }

        async void Submit(StepModel arg)
        {
            try
            {
                var confirm = await _dialogService.Confirm("Bạn chắc chắn muốn lưu thông tin?", "Tạo mới profile", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

                var ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(Model.C001);
                var ovenInfo = ovensInfo.FirstOrDefault(x => x.Id == OvenId);
                var profileInfo = ovenInfo.Profiles.FirstOrDefault(x => x.Id == ProfileId);

                if (StepId > 0)
                {
                    _stepInfo = profileInfo.Steps.FirstOrDefault(x => x.Id == StepId);

                    _stepInfo.Id = arg.Id;
                    _stepInfo.StepType = arg.StepType;
                    _stepInfo.SetPoint = arg.SetPoint;
                    _stepInfo.Hours = arg.Hours;
                    _stepInfo.Minutes = arg.Minutes;
                    _stepInfo.Seconds = arg.Seconds;
                }
                else
                {
                    if (profileInfo.Steps == null)
                        profileInfo.Steps = new List<StepModel>();

                    profileInfo.Steps.Add(arg);
                }

                Model.C001 = JsonConvert.SerializeObject(ovensInfo);

                var res = await _ft01Client.UpdateAsync(Model);

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Cập nhật không thành công.",
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
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 40000
                });
                return;
            }
        }
    }
}
