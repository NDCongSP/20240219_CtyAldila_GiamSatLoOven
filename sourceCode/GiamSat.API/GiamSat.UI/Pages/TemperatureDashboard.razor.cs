using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Threading.Tasks;

namespace GiamSat.UI.Pages
{
    public partial class TemperatureDashboard : IDisposable
    {
        [Inject] private ITemperatureDataClient _temperatureDataClient { get; set; } = default!;
        [Inject] private ITemperatureConfigClient _temperatureConfigClient { get; set; } = default!;
        
        private ICollection<TemperatureRealtimeModel>? _realtimeData;
        private System.Timers.Timer? _timer;
        private bool _isFirstLoad = true;
        private double _timeBlinkAlarm = 1000;
        private double _intervalRealtime = 1000;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var globalConfig = await _temperatureConfigClient.GetConfigsAsync();
                if (globalConfig != null)
                {
                    _timeBlinkAlarm = globalConfig.TimeBlinkAlarm;
                    _intervalRealtime = globalConfig.IntervalRealtimeUI > 0 ? globalConfig.IntervalRealtimeUI : 1000;
                }
            }
            catch {}

            _timer = new System.Timers.Timer(_intervalRealtime);
            _timer.Elapsed += async (sender, e) => await Timer_Elapsed();
            _timer.AutoReset = true;
            _timer.Start();

            // Load lần đầu tiên
            await LoadData();
        }

        private async Task Timer_Elapsed()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var response = await _temperatureDataClient.GetRealtimeAsync();
                
                await InvokeAsync(() =>
                {
                    _realtimeData = response;
                    if (_isFirstLoad) _isFirstLoad = false;
                    StateHasChanged();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetch realtime dashboard: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }
    }
}
