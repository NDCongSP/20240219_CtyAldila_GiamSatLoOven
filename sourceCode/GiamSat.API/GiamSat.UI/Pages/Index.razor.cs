using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Excel;
using GiamSat.Models;
using GiamSat.UI.Components;
using Newtonsoft.Json;
using Radzen;
using System.Globalization;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class Index
    {
        private RealtimeDisplays? _displayRealtime;

        private System.Timers.Timer _timer;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //if (firstRender)
            //{
            //    //await RadzenChart.Resize(width:100, height: 200);
            //    _timer.Interval = 1000;
            //    _timer.Elapsed += RefreshData;

            //    _timer.Enabled = true;
            //}

            //await base.OnAfterRenderAsync(firstRender);
        }
        protected override async Task OnInitializedAsync()
        {
            try
            {
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
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 4000 });
                        return;
                    }

                    _displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);
                }
                else
                {
                    foreach (var item in res.Messages)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = item, Duration = 4000 });
                    }
                }

                #region Timer refresh data
                _timer = new System.Timers.Timer(GlobalVariable.ConfigSystem.RefreshInterval);
                _timer.Elapsed += RefreshData;
                _timer.Enabled = true;
                #endregion
            }
            catch { }
        }

        private async void RefreshData(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var res = await _ft02Client.GetAllAsync();

                if (res.Succeeded)
                {
                    var _dataFromDB = res.Data.ToList();

                    if (_dataFromDB == null && _dataFromDB.Count <= 0)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 4000 });
                        return;
                    }

                    _displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);
                }

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT
            }
            catch { }
        }

        private async void OnClick(int ovenId, string ovenName)
        {
            await _dialogService.OpenAsync<DialogCardPageOvenDetail>(ovenName,
              new Dictionary<string, object>() { { "OvenId", ovenId } },
              new DialogOptions() { Width = "1500px", Height = "700px", Resizable = true, Draggable = true ,CloseDialogOnOverlayClick=true}
              );
        }
    }
}
