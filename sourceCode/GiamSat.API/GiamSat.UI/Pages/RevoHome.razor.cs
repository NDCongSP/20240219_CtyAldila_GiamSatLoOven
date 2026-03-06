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

        // Key = RevoId, Value = (currentHourShafts, prevHourShafts)
        private Dictionary<int, (int Current, int Prev)> _shaftStats = new();

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

            // Load shaft stats sau khi đã có danh sách RevoId (ngoài try-catch chính)
            await LoadShaftStats();
        }

        private async Task LoadShaftStats()
        {
            try
            {
                var now = DateTime.Now;
                var currentHourStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                var currentHourEnd   = currentHourStart.AddHours(1);
                var prevHourStart    = currentHourStart.AddHours(-1);

                // Query toàn bộ trong 2 giờ (current + prev) theo tất cả REVO
                var filter = new APIClient.RevoFilterModel
                {
                    GetAll  = true,
                    FromDate = prevHourStart,
                    ToDate   = currentHourEnd
                };
                var result = await _fT09Client.GetFilterAsync(filter);
                if (result.Succeeded && result.Data != null)
                {
                    // Group by RevoId, then split by hour bucket
                    var newStats = new Dictionary<int, (int Current, int Prev)>();
                    var grouped = result.Data
                        .Where(x => x.RevoId.HasValue)
                        .GroupBy(x => x.RevoId!.Value);

                    foreach (var g in grouped)
                    {
                        var revoId = g.Key;
                        var currentShafts = g
                            .Where(x => x.CreatedAt.HasValue
                                     && x.CreatedAt.Value >= currentHourStart
                                     && x.CreatedAt.Value < currentHourEnd
                                     && x.ShaftNum.HasValue)
                            .Select(x => x.ShaftNum!.Value)
                            .Distinct().Count();

                        var prevShafts = g
                            .Where(x => x.CreatedAt.HasValue
                                     && x.CreatedAt.Value >= prevHourStart
                                     && x.CreatedAt.Value < currentHourStart
                                     && x.ShaftNum.HasValue)
                            .Select(x => x.ShaftNum!.Value)
                            .Distinct().Count();

                        newStats[revoId] = (currentShafts, prevShafts);
                    }

                    _shaftStats = newStats;
                    StateHasChanged();
                }
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

            await _dialogService.OpenAsync<DialogRevoDetail>("Chi tiết REVO",
                new Dictionary<string, object>
                {
                    { "RevoData",          revo },
                    { "ShaftCurrentCount", stats.Current },
                    { "ShaftPrevCount",    stats.Prev },
                    { "PrevHour",          prevHour }
                },
                new DialogOptions { Width = dialogWidth, Height = dialogHeight, Resizable = true, Draggable = true });
        }
    }
}
