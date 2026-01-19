using GiamSat.Models;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using System.Net.Http;
using System.Net.Http.Json;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class RevoHome : IDisposable
    {
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;

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

                var httpClient = HttpClientFactory.CreateClient("GiamSatAPI");

                // Load FT08 data
                var ft08Response = await httpClient.GetAsync("api/FT08");
                if (ft08Response.IsSuccessStatusCode)
                {
                    var ft08Result = await ft08Response.Content.ReadFromJsonAsync<Result<List<FT08_RevoRealtime>>>();
                    if (ft08Result != null && ft08Result.Succeeded && ft08Result.Data != null)
                    {
                        _revoData = new List<RevoRealtimeModel>();

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
                }

                // Load FT07 to get REVO names
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
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi tải dữ liệu: {ex.Message}");
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
            
            await DialogService.OpenAsync<DialogRevoDetail>("Chi tiết REVO",
                new Dictionary<string, object> { { "RevoData", revo } },
                new DialogOptions { Width = dialogWidth, Height = dialogHeight, Resizable = true, Draggable = true });
        }
    }
}
