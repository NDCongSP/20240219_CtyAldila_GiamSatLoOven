using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Timers;
using Newtonsoft.Json;

namespace GiamSat.UI.Components
{
    public partial class DialogRevoDetail : IDisposable
    {
        [Parameter]
        public RevoRealtimeModel RevoData { get; set; } = new RevoRealtimeModel();

        private RevoRealtimeModel _revoData = new RevoRealtimeModel();
        private System.Timers.Timer? _refreshTimer;

        protected override void OnParametersSet()
        {
            _revoData = RevoData;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadRevoData();
            
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
                await LoadRevoData();
            });
        }

        private async Task LoadRevoData()
        {
            try
            {
                var revoId = _revoData.RevoId;

                // Load FT08 data via NSwag client
                var ft08Result = await _fT08Client.GetAllAsync();
                if (ft08Result.Succeeded && ft08Result.Data != null)
                {
                    var ft08 = ft08Result.Data.FirstOrDefault(x => x.C000_RevoId == revoId);
                    if (ft08 != null && !string.IsNullOrEmpty(ft08.C001_Data))
                    {
                        var revoRealtime = JsonConvert.DeserializeObject<RevoRealtimeModel>(ft08.C001_Data);
                        if (revoRealtime != null)
                        {
                            revoRealtime.RevoId = ft08.C000_RevoId ?? 0;

                            // Load FT07 to get REVO name via NSwag client
                            var ft07Result = await _fT07Client.GetAllAsync();
                            if (ft07Result.Succeeded && ft07Result.Data != null && ft07Result.Data.Count > 0)
                            {
                                var ft07 = ft07Result.Data.FirstOrDefault(x => x.Actived == true) ?? ft07Result.Data.FirstOrDefault();
                                if (ft07 != null && !string.IsNullOrEmpty(ft07.C000))
                                {
                                    var revoConfigs = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);
                                    if (revoConfigs != null)
                                    {
                                        var config = revoConfigs.FirstOrDefault(x => x.Id == revoId);
                                        if (config != null)
                                        {
                                            revoRealtime.RevoName = config.Name ?? $"REVO {revoId}";
                                        }
                                        else
                                        {
                                            revoRealtime.RevoName = $"REVO {revoId}";
                                        }
                                    }
                                }
                            }

                            // If name not loaded, keep the existing one
                            if (string.IsNullOrEmpty(revoRealtime.RevoName))
                            {
                                revoRealtime.RevoName = _revoData.RevoName;
                            }

                            _revoData = revoRealtime;
                            StateHasChanged();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail for timer updates
                System.Diagnostics.Debug.WriteLine($"Error refreshing REVO data: {ex.Message}");
            }
        }

        private string FormatRunTime(double totalSeconds)
        {
            if (totalSeconds <= 0) return "0s";
            if (totalSeconds < 60) return $"{totalSeconds:F2}s";
            var ts = TimeSpan.FromSeconds(totalSeconds);
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h{ts.Minutes:D2}m{ts.Seconds:D2}s";
            return $"{ts.Minutes}m{ts.Seconds:D2}s";
        }

        private string GetStepClass(RevoStep step)
        {
            if (!step.Visible.HasValue || step.Visible.Value == false)
                return "";

            // Enable = false → Black
            if (step.Enable.HasValue && step.Enable.Value == false)
            {
                return "step-disabled";
            }

            // Đã chạy xong (có StartAt + EndAt) → Gray
            if (step.StartAt.HasValue && step.EndAt.HasValue)
            {
                return "step-completed";
            }

            // Đang chạy (có StartAt, chưa có EndAt) → Green
            if (step.StartAt.HasValue && !step.EndAt.HasValue)
            {
                return "step-running";
            }

            // Chưa chạy → Cyan Blue
            return "step-pending";
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }
    }
}
