using Blazorise.Utilities;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Excel;
using GiamSat.Models;
using GiamSat.Models.NotTable;
using GiamSat.UI.Components;
using Newtonsoft.Json;
using Radzen;
using System.Globalization;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class Index : IDisposable
    {
        private RealtimeDisplays _displayRealtime = new RealtimeDisplays();
        private APIClient.FT06 _ft06 = new APIClient.FT06();
        private List<ControlPlcModel> _controlPlcModel = new List<ControlPlcModel>();

        private System.Timers.Timer _timer;
        private bool _isLoad = false;

        string _styleCard = "rz-background-color-info-light rz-shadow-0 rz-border-radius-4 rz-m-0";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                //var authState = await _authSerivce.GetAuthenticationStateAsync();
                //if (authState.User.Identity == null || !authState.User.Identity.IsAuthenticated)
                //{
                //    return;
                //}

                #region Get configsystem
                var resC = await _ft01Client.GetAllAsync();
                if (resC != null)
                {
                    GlobalVariable.ConfigSystem = JsonConvert.DeserializeObject<GiamSat.Models.ConfigModel>(resC.Data.ToList().FirstOrDefault().C000);
                }
                #endregion

                var res = await _ft02Client.GetAllAsync();

                if (res.Succeeded)
                {
                    var _dataFromDB = res.Data.ToList();//FT02

                    if (_dataFromDB == null && _dataFromDB.Count <= 0)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 2000 });
                        return;
                    }

                    _displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);
                }
                else
                {
                    foreach (var item in res.Messages)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = item, Duration = 2000 });
                    }
                }

                #region get thông tin điều khiển PLC
                var r = await _ft06Client.GetAllAsync();
                if (r.Succeeded)
                {
                    _ft06 = r.Data.ToList().FirstOrDefault();

                    _controlPlcModel = JsonConvert.DeserializeObject<List<ControlPlcModel>>(_ft06.C000);
                }
                #endregion

                #region Timer refresh data
                _timer = new System.Timers.Timer(GlobalVariable.ConfigSystem.RefreshInterval);

                _timer.Elapsed += RefreshData;
                _timer.Enabled = true;
                #endregion

                _isLoad = true;
            }
            catch { }
        }

        async void OnClickOffSerien(int ovenId, string ovenName)
        {
            try
            {
                var c = _controlPlcModel.FirstOrDefault(x => x.OvenId == ovenId);

                c.OffSerien = 1;

                _ft06.C000 = JsonConvert.SerializeObject(_controlPlcModel);

                var res = await _ft06Client.UpdateAsync(_ft06);

                if (res.Succeeded)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Truyền lệnh tắt còi thành công",
                        Duration = 2000
                    });
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Truyền lệnh tắt còi thất bại.",
                        Duration = 2000
                    });
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 2000
                });

                return;
            }
        }

        private async void RefreshData(object? sender, ElapsedEventArgs e)
        {
            try
            {
                //var authState = await _authSerivce.GetAuthenticationStateAsync();
                //if (authState.User.Identity != null && authState.User.Identity.IsAuthenticated)
                {
                    var res = await _ft02Client.GetAllAsync();

                    if (res.Succeeded)
                    {
                        var _dataFromDB = res.Data.ToList();

                        if (_dataFromDB == null && _dataFromDB.Count <= 0)
                        {
                            _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 2000 });
                            return;
                        }

                        _displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);
                    }

                    StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT
                }
            }
            catch { }
        }

        private async void OnClick(int ovenId, string ovenName)
        {
            await _dialogService.OpenAsync<DialogCardPageOvenDetail>($"{ovenName} details",
              new Dictionary<string, object>() { { "OvenId", ovenId } },
              new DialogOptions() { Width = "1500px", Height = "800px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true }
              );
        }

        public void Dispose()
        {
            if (_isLoad) _timer.Dispose();
        }
    }
}
