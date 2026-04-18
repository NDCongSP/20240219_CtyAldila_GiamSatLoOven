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

        [Parameter]
        public int ShaftTotalCount { get; set; } = 0;

        [Parameter]
        public int ShaftLastHourTotalCount { get; set; } = 0;

        /// <summary>Giờ trước (number), vd 13 thì hiển thị "Last Hour (13h)"</summary>
        [Parameter]
        public int PrevHour { get; set; } = DateTime.Now.AddHours(-1).Hour;

        private RevoRealtimeModel _revoData = new RevoRealtimeModel();
        private System.Timers.Timer? _refreshTimer;

        // Live shaft counts — Current Hour
        private int _liveShaftTotal   = 0; // TotalShaftCurrentHour
        private int _liveShaftCurrent = 0; // TotalShaftFinishCurrentHour
        // Live shaft counts — Last Hour
        private int _liveShaftLastHourTotal = 0; // TotalShaftLastHour
        private int _liveShaftPrev          = 0; // TotalShaftFinshLastHour
        private int _livePrevHour = DateTime.Now.AddHours(-1).Hour;

        protected override void OnParametersSet()
        {
            _revoData               = RevoData;
            _liveShaftTotal         = ShaftTotalCount;
            _liveShaftCurrent       = ShaftCurrentCount;
            _liveShaftLastHourTotal = ShaftLastHourTotalCount;
            _liveShaftPrev          = ShaftPrevCount;
            _livePrevHour           = PrevHour;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadRevoData();

            _refreshTimer = new System.Timers.Timer(GlobalVariable.RevoRefreshInterval);
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
                var result = await _fT09Client.GetTotalShaftAsync(_revoData.RevoId);
                if (result.Succeeded && result.Data != null)
                {
                    var dto = result.Data.FirstOrDefault(x => x.RevoId == _revoData.RevoId);
                    _liveShaftTotal         = dto?.TotalShaftCurrentHour      ?? 0;
                    _liveShaftCurrent       = dto?.TotalShaftFinishCurrentHour ?? 0;
                    _liveShaftLastHourTotal = dto?.TotalShaftLastHour           ?? 0;
                    _liveShaftPrev          = dto?.TotalShaftFinshLastHour      ?? 0;
                    _livePrevHour           = DateTime.Now.AddHours(-1).Hour;
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
                                            revoRealtime.RevoName    = config.Name ?? $"REVO {revoId}";
                                            revoRealtime.MachineType = config.MachineType;
                                            GlobalVariable.RevoConfig = config;
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
            if (totalSeconds < 60) return $"{totalSeconds.ToString("0.##")}s";
            var ts = TimeSpan.FromSeconds(totalSeconds);
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h{ts.Minutes:D2}m{ts.Seconds:D2}s";
            return $"{ts.Minutes}m{ts.Seconds:D2}s";
        }

        private bool IsAutoRollingMode() =>
            _revoData.MachineType == EnumMachineType.AUTO_ROLLING;

        private string GetStepClass(RevoStep step, bool isRunning)
        {
            if (!step.Visible.HasValue || step.Visible.Value == false)
                return "";

            // Enable = false → Black
            if (step.Enable.HasValue && step.Enable.Value == false)
                return "step-disabled";

            // Đang chạy (StepIndex nhỏ nhất, TotalRunTime == 0) → Red
            if (isRunning)
                return "step-running";

            // Đã xong (TotalRunTime > 0) → Gray
            if (step.TotalRunTime.HasValue && step.TotalRunTime.Value > 0)
                return "step-completed";

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
