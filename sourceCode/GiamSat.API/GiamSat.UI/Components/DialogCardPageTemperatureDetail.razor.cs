using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageTemperatureDetail : IDisposable
    {
        [Parameter] public int LocationId { get; set; }

        [Inject] private ITemperatureDataClient _temperatureDataClient { get; set; } = default!;

        private TemperatureRealtimeModel? _displayInfo;
        private System.Timers.Timer? _timer;
        private bool _disposed = false;

        private List<ChartDataItem> _chartDataSeriesTemp = new();
        private List<ChartDataItem> _chartDataSeriesHigh = new();
        private List<ChartDataItem> _chartDataSeriesLow = new();
        private Radzen.Blazor.RadzenChart RadzenChart = new();

        private const int MaxChartPoints = 60; // 10 minutes if 10s interval

        protected override async Task OnInitializedAsync()
        {
            await LoadData();

            _timer = new System.Timers.Timer(10000); // 10 seconds refresh
            _timer.Elapsed += async (sender, e) => await RefreshData();
            _timer.AutoReset = true;
            _timer.Start();
        }

        private async Task LoadData()
        {
            try
            {
                var response = await _temperatureDataClient.GetRealtimeAsync();
                _displayInfo = response.FirstOrDefault(x => x.Id == LocationId);
                
                if (_displayInfo != null)
                {
                    AddPointToChart(_displayInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading detail: {ex.Message}");
            }
        }

        private async Task RefreshData()
        {
            if (_disposed) return;

            try
            {
                var response = await _temperatureDataClient.GetRealtimeAsync();
                _displayInfo = response.FirstOrDefault(x => x.Id == LocationId);

                if (_displayInfo != null && !_disposed)
                {
                    AddPointToChart(_displayInfo);
                    
                    await InvokeAsync(() =>
                    {
                        if (!_disposed)
                        {
                            RadzenChart.Reload();
                            StateHasChanged();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing chart: {ex.Message}");
            }
        }

        private void AddPointToChart(TemperatureRealtimeModel item)
        {
            var now = DateTime.Now;
            
            if (_chartDataSeriesTemp.Count >= MaxChartPoints)
            {
                _chartDataSeriesTemp.RemoveAt(0);
                _chartDataSeriesHigh.RemoveAt(0);
                _chartDataSeriesLow.RemoveAt(0);
            }

            _chartDataSeriesTemp.Add(new ChartDataItem { Date = now, Value = item.Pv });
            _chartDataSeriesHigh.Add(new ChartDataItem { Date = now, Value = item.Config.HightLevel });
            _chartDataSeriesLow.Add(new ChartDataItem { Date = now, Value = item.Config.LowLevel });
        }

        private string FormatAsTime(object value)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("HH:mm:ss");
            }
            return value?.ToString() ?? "";
        }

        private string FormatAsNumber(object value)
        {
            return ((double)value).ToString("0.0");
        }

        public void Dispose()
        {
            _disposed = true;
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        public class ChartDataItem
        {
            public DateTime Date { get; set; }
            public double Value { get; set; }
        }
    }
}
