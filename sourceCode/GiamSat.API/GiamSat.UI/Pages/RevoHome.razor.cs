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
        private bool _disposed;

        // Key = RevoId, Value = shaft count DTO from sp_GetTotalShaft
        private Dictionary<int, APIClient.RevoGetTotalShaftCountDto> _shaftStats = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            
            _refreshTimer = new System.Timers.Timer(GlobalVariable.RevoRefreshInterval);
            _refreshTimer.Elapsed += OnTimerElapsed;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_disposed) return;
            await InvokeAsync(async () =>
            {
                if (_disposed) return;
                await LoadData();
            });
        }

        public void Dispose()
        {
            _disposed = true;
            if (_refreshTimer != null)
            {
                _refreshTimer.Elapsed -= OnTimerElapsed;
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
        }

        private async Task LoadData()
        {
            if (_disposed) return;
            try
            {
                _isLoading = true;
                if (!_disposed) StateHasChanged();

                _revoData = new List<RevoRealtimeModel>();

                // Load FT08 data via NSwag client
                var ft08Result = await _fT08Client.GetAllAsync();
                if (_disposed) return;
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
                if (_disposed) return;
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
                                    revo.RevoName    = config.Name ?? $"REVO {revo.RevoId}";
                                    revo.MachineType = config.MachineType;
                                    revo.Pulse_Rev   = config.Pulse_Rev;
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
                if (!_disposed) StateHasChanged();
            }

            // Load shaft stats sau khi đã có danh sách RevoId (ngoài try-catch chính)
            await LoadShaftStats();
        }

        private async Task LoadShaftStats()
        {
            if (_disposed) return;
            try
            {
                var result = await _fT09Client.GetTotalShaftAsync(null);
                if (_disposed) return;

                var newStats = new Dictionary<int, APIClient.RevoGetTotalShaftCountDto>();
                if (result.Succeeded && result.Data != null)
                {
                    foreach (var item in result.Data)
                        newStats[item.RevoId] = item;
                }

                _shaftStats = newStats;
                if (!_disposed) StateHasChanged();
            }
            catch
            {
                // Silent fail - shaft stats is non-critical
            }
        }

        private async Task OnShowDetail(RevoRealtimeModel revo)
        {
            var dialogWidth  = "90vw";
            var dialogHeight = "90vh";

            var now = DateTime.Now;
            var prevHour = now.AddHours(-1).Hour;

            _shaftStats.TryGetValue(revo.RevoId, out var stats);
            var dto = stats ?? new APIClient.RevoGetTotalShaftCountDto { RevoId = revo.RevoId };

            // Ensure the global config is updated before opening the dialog for immediate correct display
            GlobalVariable.RevoConfig.Pulse_Rev = revo.Pulse_Rev;

            await _dialogService.OpenAsync<DialogRevoDetail>("Chi tiết REVO",
                new Dictionary<string, object>
                {
                    { "RevoData",          revo },
                    { "ShaftCurrentCount",        dto.TotalShaftFinishCurrentHour },
                    { "ShaftPrevCount",           dto.TotalShaftFinshLastHour },
                    { "ShaftTotalCount",          dto.TotalShaftCurrentHour },
                    { "ShaftLastHourTotalCount",  dto.TotalShaftLastHour },
                    { "PrevHour",          prevHour }
                },
                new DialogOptions { Width = dialogWidth, Height = dialogHeight, Resizable = true, Draggable = true });
        }
    }
}
