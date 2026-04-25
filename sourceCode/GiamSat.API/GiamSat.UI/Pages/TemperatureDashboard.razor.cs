using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GiamSat.UI.Pages
{
    public partial class TemperatureDashboard : IDisposable
    {
        [Inject] private ITemperatureDataClient _temperatureDataClient { get; set; } = default!;
        
        private ICollection<TemperatureRealtimeModel>? _realtimeData;
        private Timer? _timer;
        private bool _isFirstLoad = true;

        protected override void OnInitialized()
        {
            _timer = new Timer(async (e) =>
            {
                await LoadData();
            }, null, 0, 1000); // Trigger every 1s
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
            _timer?.Dispose();
        }
    }
}
