using DocumentFormat.OpenXml.Drawing;
using GiamSat.Models;
using Newtonsoft.Json;
using Radzen;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class Index
    {
        private List<APIClient.FT02>? _dataFromDB;
        private RealtimeDisplays? _displayRealtime;

        private System.Timers.Timer _timer;

        bool visible = true;


        protected override async Task OnInitializedAsync()
        {
            try
            {
                var res = await _ft02Client.GetAllAsync();

                if (res.Succeeded)
                {
                    _dataFromDB = res.Data.ToList();

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
                _timer = new System.Timers.Timer(GlobalVariable.RefreshInterval);
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
                    _dataFromDB = res.Data.ToList();

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
    }
}
