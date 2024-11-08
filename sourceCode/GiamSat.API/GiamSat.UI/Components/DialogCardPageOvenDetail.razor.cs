﻿using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Timers;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageOvenDetail:IDisposable
    {
        [Parameter] public int OvenId { get; set; }

        RealtimeDisplayModel _ovenDisplayInfo { get; set; } = new RealtimeDisplayModel();
        private System.Timers.Timer _timer;

        Radzen.Blazor.RadzenChart RadzenChart = new Radzen.Blazor.RadzenChart();
        List<DataItem> _chartDataSeriesTemp = new List<DataItem>();        
        List<DataItem> _chartDataSeriesSetpoint = new List<DataItem>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                var res = await _ft02Client.GetAllAsync();

                if (res.Succeeded)
                {
                    var _dataFromDB = res.Data.ToList();//FT02

                    if (_dataFromDB == null && _dataFromDB.Count <= 0)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 2000 });
                        return;
                    }

                    var displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);
                    _ovenDisplayInfo = displayRealtime.FirstOrDefault(x => x.OvenId == OvenId);
                }
                else
                {
                    foreach (var item in res.Messages)
                    {
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = item, Duration = 2000 });
                    }
                }

                #region Timer refresh data
                _timer = new System.Timers.Timer(GlobalVariable.ConfigSystem.ChartRefreshInterval);
                _timer.Elapsed += RefreshData;
                _timer.Enabled = true;
                #endregion
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = ex.Message, Duration = 2000 });
                return;
            }
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
                        _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Warning", Detail = "Data empty", Duration = 2000 });
                        return;
                    }

                    var displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(_dataFromDB.FirstOrDefault().C000);
                    _ovenDisplayInfo = displayRealtime.FirstOrDefault(x => x.OvenId == OvenId);
                }

                if (_chartDataSeriesTemp.Count >= GlobalVariable.ConfigSystem.ChartPointNum)
                {
                    _chartDataSeriesTemp.RemoveAt(0);
                    _chartDataSeriesSetpoint.RemoveAt(0);
                }

                string date = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss");

                //add data vao 2 series
                _chartDataSeriesTemp.Add(new DataItem()
                {
                    Date = date,
                    Temperature = _ovenDisplayInfo.Temperature
                });
                _chartDataSeriesSetpoint.Add(new DataItem()
                {
                    Date = date,
                    Temperature = _ovenDisplayInfo.SetPoint
                });

                await RadzenChart.Reload();

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT
            }
            catch { }
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

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
