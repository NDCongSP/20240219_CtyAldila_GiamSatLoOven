using DocumentFormat.OpenXml.Office2016.Excel;
using GiamSat.Models;
using Newtonsoft.Json;
using Radzen;
using System.Globalization;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class RealtimeTrend
    {
        bool smooth = false;
        bool showDataLabels = false;
        bool showMarkers = true;

        private System.Timers.Timer _timer = new System.Timers.Timer();

        Radzen.Blazor.RadzenChart RadzenChart = new Radzen.Blazor.RadzenChart();

        private RealtimeDisplays _displayRealtime = new RealtimeDisplays();

        double _so1 = 100,_so2=100;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await RadzenChart.Resize(width:100, height: 200);
                _timer.Interval = 1000;
                _timer.Elapsed += Refresh;

                _timer.Enabled = true;
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async void Refresh(object? sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Enabled = false;

                string date = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss");

                var res = await _ft02Client.GetAllAsync();
                if (!res.Succeeded)
                {
                    foreach (var item in res.Messages)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = item, Duration = 4000 });
                    }
                    return;
                }

                var _dataFromDB = res.Data.ToList();//FT02

                if (_dataFromDB == null && _dataFromDB.Count <= 0)
                {
                    _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 4000 });
                    return;
                }

                _displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);

                _so1 = _so1 + 10.5;
                _so2 = _so2 + 15.5;

                if (oven1.Count >= 30)
                {
                    oven1.RemoveAt(0);
                    oven2.RemoveAt(0);
                }

                oven1.Add(new DataItem()
                {
                    Date = date,
                    Temperature = _displayRealtime[0].Temperature
                });

                oven2.Add(new DataItem()
                {
                    Date = date,
                    Temperature = _displayRealtime[1].Temperature,
                });

                await RadzenChart.Reload();

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT

                _timer.Enabled = true;
            }
            catch (Exception ex)
            {
                _timer.Enabled = true;
            }
        }

        string FormatAsUSD(object value)
        {
            return ((double)value).ToString();
        }

        string FormatAsMonth(object value)
        {
            if (value != null)
            {
                return Convert.ToDateTime(value).ToString("HH:mm:ss");
            }

            return string.Empty;
        }

        List<DataItem> oven1 = new List<DataItem>() {
            new DataItem
            {
                Date = ("2024-03-22 14:20:00"),
                Temperature = 140
            },
            new DataItem
            {
                Date = ("2024-03-22 14:21:10"),
                Temperature = 141
            },
        };
        List<DataItem> oven2 = new List<DataItem>()
        {
            new DataItem
            {
                Date = ("2024-03-22 14:20:00"),
                Temperature = 150
            },
            new DataItem
            {
                Date = ("2024-03-22 14:21:10"),
                Temperature = 151
            },
        };
    }
}
