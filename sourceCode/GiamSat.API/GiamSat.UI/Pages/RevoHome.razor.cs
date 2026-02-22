using GiamSat.Models;
using GiamSat.UI.Components;
using Newtonsoft.Json;
using Radzen;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class RevoHome : IDisposable
    {
        private List<RevoRealtimeModel> _revoData = new List<RevoRealtimeModel>();
        private bool _isLoading = true;
        private System.Timers.Timer? _refreshTimer;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            
            // Setup timer to refresh every 10 seconds
            _refreshTimer = new System.Timers.Timer(10000); // 10 seconds
            _refreshTimer.Elapsed += OnTimerElapsed;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            await InvokeAsync(async () =>
            {
                await LoadData();
            });
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        private async Task LoadData()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                _revoData = new List<RevoRealtimeModel>();

                // Load FT08 data via NSwag client
                var ft08Result = await _fT08Client.GetAllAsync();
                if (ft08Result.Succeeded && ft08Result.Data != null)
                {
                    foreach (var ft08 in ft08Result.Data)
                    {
                        if (!string.IsNullOrEmpty(ft08.C001_Data))
                        {
                            var revoRealtime = JsonConvert.DeserializeObject<RevoRealtimeModel>(ft08.C001_Data);
                            if (revoRealtime != null)
                            {
                                revoRealtime.RevoId = ft08.C000_RevoId ?? 0;
                                _revoData.Add(revoRealtime);
                            }
                        }
                    }
                }

                // Load FT07 to get REVO names via NSwag client
                var ft07Result = await _fT07Client.GetAllAsync();
                if (ft07Result.Succeeded && ft07Result.Data != null && ft07Result.Data.Count > 0)
                {
                    var ft07 = ft07Result.Data.FirstOrDefault(x => x.Actived == true) ?? ft07Result.Data.FirstOrDefault();
                    if (ft07 != null && !string.IsNullOrEmpty(ft07.C000))
                    {
                        var revoConfigs = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);
                        if (revoConfigs != null)
                        {
                            foreach (var revo in _revoData)
                            {
                                var config = revoConfigs.FirstOrDefault(x => x.Id == revo.RevoId);
                                if (config != null)
                                {
                                    revo.RevoName = config.Name ?? $"REVO {revo.RevoId}";
                                }
                                else
                                {
                                    revo.RevoName = $"REVO {revo.RevoId}";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi tải dữ liệu: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private async Task OnShowDetail(RevoRealtimeModel revo)
        {
            // Calculate dialog size: 90% of viewport (10% smaller than screen)
            var dialogWidth = "90vw";
            var dialogHeight = "90vh";
            
            await _dialogService.OpenAsync<DialogRevoDetail>("Chi tiết REVO",
                new Dictionary<string, object> { { "RevoData", revo } },
                new DialogOptions { Width = dialogWidth, Height = dialogHeight, Resizable = true, Draggable = true });
        }
    }
}
