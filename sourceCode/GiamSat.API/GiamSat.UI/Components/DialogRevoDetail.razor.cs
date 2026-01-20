using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http;
using System.Net.Http.Json;
using System.Timers;
using Newtonsoft.Json;

namespace GiamSat.UI.Components
{
    public partial class DialogRevoDetail : IDisposable
    {
        [Parameter]
        public RevoRealtimeModel RevoData { get; set; } = new RevoRealtimeModel();

        [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;

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
                var httpClient = HttpClientFactory.CreateClient("GiamSatAPI");
                var revoId = _revoData.RevoId;

                // Load FT08 data for this specific REVO
                var ft08Response = await httpClient.GetAsync("api/FT08");
                if (ft08Response.IsSuccessStatusCode)
                {
                    var ft08Result = await ft08Response.Content.ReadFromJsonAsync<Result<List<FT08_RevoRealtime>>>();
                    if (ft08Result != null && ft08Result.Succeeded && ft08Result.Data != null)
                    {
                        var ft08 = ft08Result.Data.FirstOrDefault(x => x.C000_RevoId == revoId);
                        if (ft08 != null && !string.IsNullOrEmpty(ft08.C001_Data))
                        {
                            var revoRealtime = JsonConvert.DeserializeObject<RevoRealtimeModel>(ft08.C001_Data);
                            if (revoRealtime != null)
                            {
                                revoRealtime.RevoId = ft08.C000_RevoId ?? 0;

                                // Load FT07 to get REVO name (in case it changed)
                                var ft07Response = await httpClient.GetAsync("api/FT07");
                                if (ft07Response.IsSuccessStatusCode)
                                {
                                    var ft07Result = await ft07Response.Content.ReadFromJsonAsync<Result<List<FT07_RevoConfig>>>();
                                    if (ft07Result != null && ft07Result.Succeeded && ft07Result.Data != null && ft07Result.Data.Count > 0)
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
            }
            catch (Exception ex)
            {
                // Silent fail for timer updates
                System.Diagnostics.Debug.WriteLine($"Error refreshing REVO data: {ex.Message}");
            }
        }

        private string GetStepClass(RevoStep step)
        {
            if (!step.Visible.HasValue || step.Visible.Value == false)
                return "";

            var now = DateTime.Now;
            var classes = new List<string>();

            // Enable status - if disabled, always black
            if (step.Enanble.HasValue && step.Enanble.Value == false)
            {
                classes.Add("step-disabled"); // Black
            }
            else
            {
                // Check for running step (startAt != null, endAt == null)
                if (step.StartAt.HasValue && !step.EndAt.HasValue)
                {
                    classes.Add("step-running"); // White - running
                }
                // Time-based status
                else if (step.StartAt.HasValue && step.EndAt.HasValue)
                {
                    if (now < step.StartAt.Value)
                    {
                        classes.Add("step-pending"); // Gray - not started
                    }
                    else if (now >= step.StartAt.Value && now <= step.EndAt.Value)
                    {
                        classes.Add("step-running"); // White - running
                    }
                    else
                    {
                        classes.Add("step-completed"); // Light blue - completed
                    }
                }
                else
                {
                    // No time info, default to pending
                    classes.Add("step-pending");
                }
            }

            return string.Join(" ", classes);
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }
    }
}
