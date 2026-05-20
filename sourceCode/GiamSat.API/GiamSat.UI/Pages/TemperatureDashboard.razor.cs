using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Radzen;
using GiamSat.UI.Components;

namespace GiamSat.UI.Pages
{
    public partial class TemperatureDashboard : IDisposable
    {
        [Inject] private ITemperatureDataClient _temperatureDataClient { get; set; } = default!;
        [Inject] private ITemperatureConfigClient _temperatureConfigClient { get; set; } = default!;
        [Inject] private NavigationManager _navigationManager { get; set; } = default!;
        
        private ICollection<TemperatureRealtimeModel>? _realtimeData;
        private System.Timers.Timer? _timer;
        private bool _isFirstLoad = true;
        private double _timeBlinkAlarm = 1000;
        private double _intervalRealtime = 1000;
        private bool _disposed = false;

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
            if (_disposed) return;
            if (_timer != null) _timer.Stop();
            
            await LoadData();
            
            if (!_disposed && _timer != null) 
            {
                try 
                {
                    _timer.Start();
                }
                catch (ObjectDisposedException) 
                {
                    // Ignore if already disposed
                }
            }
        }

        private async Task LoadData()
        {
            try
            {
                var response = await _temperatureDataClient.GetRealtimeAsync();
                
                await InvokeAsync(() =>
                {
                    if (_disposed) return;
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

        private async Task OnViewDetail(TemperatureRealtimeModel item)
        {
            await _dialogService.OpenAsync<DialogCardPageTemperatureDetail>($"Chi tiết: {item.Name}",
                new Dictionary<string, object?> { { "LocationId", item.Id } },
                new DialogOptions { Width = "1100px", Height = "650px", Resizable = true, Draggable = true });
        }

        public void Dispose()
        {
            _disposed = true;
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
