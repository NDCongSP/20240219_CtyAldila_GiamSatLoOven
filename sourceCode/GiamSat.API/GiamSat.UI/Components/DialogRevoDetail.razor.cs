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

        [Parameter]
        public int ShaftCurrentCount { get; set; } = 0;

        [Parameter]
        public int ShaftPrevCount { get; set; } = 0;

        /// <summary>Giờ trước (number), vd 13 thì hiển thị "Total Shaft At 13h"</summary>
        [Parameter]
        public int PrevHour { get; set; } = DateTime.Now.AddHours(-1).Hour;

        private RevoRealtimeModel _revoData = new RevoRealtimeModel();
        private System.Timers.Timer? _refreshTimer;

        // Live shaft counts (refreshed every timer tick from FT09)
        private int _liveShaftCurrent = 0;
        private int _liveShaftPrev = 0;
        private int _livePrevHour = DateTime.Now.AddHours(-1).Hour;

        protected override void OnParametersSet()
        {
            _revoData = RevoData;
            _liveShaftCurrent = ShaftCurrentCount;
            _liveShaftPrev = ShaftPrevCount;
            _livePrevHour = PrevHour;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadRevoData();

            _refreshTimer = new System.Timers.Timer(10000);
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

        private async Task LoadShaftCountsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var currentHourStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                var currentHourEnd   = currentHourStart.AddHours(1);
                var prevHourStart    = currentHourStart.AddHours(-1);

                var filter = new APIClient.RevoFilterModel
                {
                    GetAll   = true,
                    FromDate = prevHourStart,
                    ToDate   = currentHourEnd
                };

                var result = await _fT09Client.GetFilterAsync(filter);
                if (result.Succeeded && result.Data != null)
                {
                    var revoId = _revoData.RevoId;
                    var records = result.Data.Where(x => x.RevoId == revoId).ToList();

                    _liveShaftCurrent = records
                        .Where(x => x.CreatedAt.HasValue
                                 && x.CreatedAt.Value >= currentHourStart
                                 && x.CreatedAt.Value < currentHourEnd
                                 && x.ShaftNum.HasValue)
                        .Select(x => x.ShaftNum!.Value)
                        .Distinct().Count();

                    _liveShaftPrev = records
                        .Where(x => x.CreatedAt.HasValue
                                 && x.CreatedAt.Value >= prevHourStart
                                 && x.CreatedAt.Value < currentHourStart
                                 && x.ShaftNum.HasValue)
                        .Select(x => x.ShaftNum!.Value)
                        .Distinct().Count();

                    _livePrevHour = prevHourStart.Hour;
                }
            }
            catch
            {
                // Silent fail – keep previous values
            }
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing REVO data: {ex.Message}");
            }

            await LoadShaftCountsAsync();
            StateHasChanged();
        }

        private string FormatRunTime(double totalSeconds)
        {
            if (totalSeconds <= 0) return "0s";
            if (totalSeconds < 60) return $"{totalSeconds:F2}s";
            var ts = TimeSpan.FromSeconds(totalSeconds);
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h{ts.Minutes:D2}m{ts.Seconds:D2}s";
            return $"{ts.Minutes}m{ts.Seconds:D2}s";
        }

        private bool IsAutoRollingMode()
        {
            var revo = _revoData.RevoName ?? string.Empty;
            var work = _revoData.Work ?? string.Empty;
            return revo.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || work.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || revo.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase)
                || work.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase);
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
