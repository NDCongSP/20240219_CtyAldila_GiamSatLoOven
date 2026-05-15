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
        [Inject] private ITemperatureConfigClient _temperatureConfigClient { get; set; } = default!;

        private TemperatureRealtimeModel? _displayInfo;
        private System.Timers.Timer? _timer;
        private bool _disposed = false;

        private List<ChartDataItem> _chartDataSeriesTemp = new();
        private List<ChartDataItem> _chartDataSeriesHigh = new();
        private List<ChartDataItem> _chartDataSeriesLow = new();
        private Radzen.Blazor.RadzenChart RadzenChart = new();

        private const int MaxChartPoints = 20;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                var response = await _temperatureDataClient.GetRealtimeAsync();
                _displayInfo = response.FirstOrDefault(x => x.Id == LocationId);

                AddPointToChart();

                // Initialize timer
                double interval = 10000;
                var config = await _temperatureConfigClient.GetConfigsAsync();
                if (config?.IntervalRealtimeDetailUI > 0)
                {
                    interval = config.IntervalRealtimeDetailUI;
                }

                _timer = new System.Timers.Timer(interval);
                _timer.Elapsed += RefreshData;
                _timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading initial detail: {ex.Message}");
            }
        }

        private async void RefreshData(object? sender, ElapsedEventArgs e)
        {
            if (_disposed) return;

            try
            {
                var response = await _temperatureDataClient.GetRealtimeAsync();

                if (_disposed) return;

                _displayInfo = response.FirstOrDefault(x => x.Id == LocationId);

                if (_displayInfo != null)
                {
                    AddPointToChart();

                    if (!_disposed)
                    {
                        await InvokeAsync(async () =>
                        {
                            if (_disposed) return;
                            await RadzenChart.Reload();
                            StateHasChanged();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing chart: {ex.Message}");
            }
        }

        private void AddPointToChart()
        {
            if (_displayInfo == null) return;

            if (_chartDataSeriesTemp.Count >= MaxChartPoints)
            {
                _chartDataSeriesTemp.RemoveAt(0);
                _chartDataSeriesHigh.RemoveAt(0);
                _chartDataSeriesLow.RemoveAt(0);
            }

            // Add zero-width space to prevent Radzen from parsing this as DateTime 
            // and creating an interpolated time axis. We want exactly 1 tick per point.
            var callTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\u200B";

            var highLevel = _displayInfo.Config?.HightLevel ?? 0;
            var lowLevel = _displayInfo.Config?.LowLevel ?? 0;

            _chartDataSeriesTemp.Add(new ChartDataItem { Date = callTime, Value = _displayInfo.Pv });
            _chartDataSeriesHigh.Add(new ChartDataItem { Date = callTime, Value = highLevel });
            _chartDataSeriesLow.Add(new ChartDataItem { Date = callTime, Value = lowLevel });
        }

        private string FormatAsTime(object value)
        {
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
                _timer.Elapsed -= RefreshData;
                _timer.Stop();
                _timer.Dispose();
            }
        }

        public class ChartDataItem
        {
            public string Date { get; set; } = string.Empty;
            public double Value { get; set; }
        }
    }
}
