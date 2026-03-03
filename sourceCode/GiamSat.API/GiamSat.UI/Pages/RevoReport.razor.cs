using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Net.Http;
using System.Net.Http.Json;
using ApiClient = GiamSat.APIClient;

namespace GiamSat.UI.Pages
{
    public partial class RevoReport
    {
        // Keep IHttpClientFactory only for PDF export (needs byte[] response)
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;

        private RadzenDataGrid<RevoStepRow> _stepGrid = default!;
        private RadzenDataGrid<RevoShaftRow> _shaftGrid = default!;
        private RadzenDataGrid<RevoHourRow> _hourGrid = default!;
        private DateTime _fromDate = DateTime.Now.AddDays(-7);
        private DateTime _toDate = DateTime.Now;
        private int? _selectedRevoId = null;
        private bool _isLoading = false;
        private bool _hasData = false;

        private List<RevoDropdownModel> _revoList = new();
        private List<FT09_RevoDatalog> _rawData = new();
        private List<RevoStepRow> _stepRows = new();
        private List<RevoShaftRow> _shaftRows = new();
        private List<RevoHourRow> _hourRows = new();
        private bool _gridInitialized = false;
        private int _shaftCount = 0;

        private RevoReportMode _reportMode = RevoReportMode.ByStep;
        private List<ReportModeOption> _reportModes = new()
        {
            new ReportModeOption { Name = "Theo step", Value = RevoReportMode.ByStep },
            new ReportModeOption { Name = "Theo shaft", Value = RevoReportMode.ByShaft },
            new ReportModeOption { Name = "Theo giờ", Value = RevoReportMode.ByHour }
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadRevoList();
        }

        private async Task LoadRevoList()
        {
            try
            {
                // Load FT07 via NSwag client
                var ft07Result = await _fT07Client.GetAllAsync();
                if (ft07Result.Succeeded && ft07Result.Data != null && ft07Result.Data.Count > 0)
                {
                    var ft07 = ft07Result.Data.FirstOrDefault(x => x.Actived == true) ?? ft07Result.Data.FirstOrDefault();
                    if (ft07 != null && !string.IsNullOrEmpty(ft07.C000))
                    {
                        var revoConfigs = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);
                        if (revoConfigs != null)
                        {
                            _revoList = revoConfigs.Select(x => new RevoDropdownModel
                            {
                                Id = x.Id ?? 0,
                                Name = x.Name ?? $"REVO {x.Id}"
                            }).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", $"Không thể tải danh sách REVO: {ex.Message}");
            }
        }

        private string GetModeName()
        {
            return _reportMode switch
            {
                RevoReportMode.ByStep => "Theo step",
                RevoReportMode.ByShaft => "Theo shaft",
                RevoReportMode.ByHour => "Theo giờ",
                _ => "Unknown"
            };
        }

        private async Task OnSplitButtonClick(RadzenSplitButtonItem args)
        {
            if (args != null && args.Value != null)
            {
                if (Enum.TryParse<RevoReportMode>(args.Value.ToString(), out var mode))
                {
                    _reportMode = mode;
                    OnModeChanged(_reportMode);
                }
            }
            await ExecuteSearch();
        }

        private async Task ExecuteSearch()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                // Use NSwag client for FT09 GetFilter
                var filterModel = new ApiClient.RevoFilterModel
                {
                    GetAll = !_selectedRevoId.HasValue,
                    RevoId = _selectedRevoId,
                    FromDate = _fromDate,
                    ToDate = _toDate
                };

                var result = await _fT09Client.GetFilterAsync(filterModel);
                if (result.Succeeded && result.Data != null)
                {
                    // Map from APIClient model to Domain model for grid binding
                    _rawData = result.Data
                        .Select(x => new FT09_RevoDatalog
                        {
                            Id = x.Id,
                            CreatedAt = x.CreatedAt,
                            CreatedMachine = x.CreatedMachine,
                            RevoId = x.RevoId,
                            RevoName = x.RevoName,
                            Work = x.Work,
                            Part = x.Part,
                            Rev = x.Rev,
                            ColorCode = x.ColorCode,
                            Mandrel = x.Mandrel,
                            MandrelStart = x.MandrelStart,
                            StepId = x.StepId,
                            StepName = x.StepName,
                            StartedAt = x.StartedAt,
                            EndedAt = x.EndedAt,
                            ShaftNum = x.ShaftNum,
                            TotalTime = x.TotalTime
                        })
                        .OrderBy(x => x.StartedAt ?? x.CreatedAt ?? DateTime.MinValue)
                        .ToList();

                    RebuildView();
                    _hasData = _rawData.Any();
                    // Đếm theo số shaft thực sự hiển thị trong view (đã có StartedAt hợp lệ)
                    _shaftCount = _shaftRows.Select(x => x.ShaftNum).Distinct().Count();
                    _gridInitialized = false; // Reset to apply grouping on next render (step mode)
                }
                else
                {
                    _rawData = new List<FT09_RevoDatalog>();
                    _stepRows = new();
                    _shaftRows = new();
                    _hourRows = new();
                    _hasData = false;
                    if (result.Messages != null && result.Messages.Count > 0)
                    {
                        var errorMessage = string.Join(", ", result.Messages);
                        _notificationService.Notify(NotificationSeverity.Error, "Lỗi", errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi tải dữ liệu: {ex.Message}");
                _hasData = false;
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private void OnReset()
        {
            _fromDate = DateTime.Now.AddDays(-7);
            _toDate = DateTime.Now;
            _selectedRevoId = null;
            _hasData = false;
            _shaftCount = 0;
            _rawData.Clear();
            _stepRows.Clear();
            _shaftRows.Clear();
            _hourRows.Clear();
            StateHasChanged();
        }

        private void OnModeChanged(object value)
        {
            // Radzen passes selected value (enum)
            _gridInitialized = false;
            if (_rawData.Count > 0)
            {
                RebuildView();
            }
            StateHasChanged();
        }

        private async Task OnExportExcel()
        {
            try
            {
                if (!_hasData || _rawData.Count == 0)
                {
                    _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất");
                    return;
                }

                var excel = new ExcelExportRevo();
                var excelBytes = excel.GenerateExcelFileAsync(_rawData, $"{_fromDate:dd/MM/yyyy} đến {_toDate:dd/MM/yyyy}", _reportMode);
                var filename = $"BaoCao_REVO_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                await _js.InvokeVoidAsync("BlazorDownloadFile", filename, excelBytes);
                _notificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã xuất file Excel thành công");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi xuất Excel: {ex.Message}");
            }
        }

        private async Task OnExportPDF()
        {
            try
            {
                if (!_hasData || _rawData.Count == 0)
                {
                    _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất");
                    return;
                }

                // Keep raw HttpClient for PDF export (NSwag returns void, we need byte[])
                var httpClient = HttpClientFactory.CreateClient("GiamSatAPI");
                var filterModel = new RevoFilterModel
                {
                    GetAll = !_selectedRevoId.HasValue,
                    RevoId = _selectedRevoId,
                    FromDate = _fromDate,
                    ToDate = _toDate
                };

                var response = await httpClient.PostAsJsonAsync("api/FT09/ExportPdf", filterModel);
                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentDisposition = response.Content.Headers.ContentDisposition;
                    var filename = contentDisposition?.FileName ?? $"BaoCao_REVO_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    
                    // Remove quotes from filename if present
                    filename = filename.Trim('"');
                    
                    await _js.InvokeVoidAsync("BlazorDownloadFile", filename, pdfBytes);
                    _notificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã xuất file PDF thành công");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Không thể xuất PDF: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi xuất PDF: {ex.Message}");
            }
        }

        private void OnStepGridRender(DataGridRenderEventArgs<RevoStepRow> args)
        {
            if (_reportMode != RevoReportMode.ByStep)
                return;

            if (!_gridInitialized && _hasData && args.Grid != null)
            {
                // Group by ShaftKey (unique string per physical shaft, e.g. "Shaft 1", "Shaft 2"...)
                args.Grid.Groups.Clear();
                args.Grid.Groups.Add(new GroupDescriptor
                {
                    Property = "ShaftKey",
                    Title = "Shaft",
                    SortOrder = SortOrder.Ascending
                });
                _gridInitialized = true;
            }
        }

        private void OnGroupRowRender(GroupRowRenderEventArgs args)
        {
            // Expand all groups by default
            args.Expanded = true;
        }

        private void RebuildView()
        {
            // Normalize
            var normalized = _rawData
                .Where(x => x != null)
                .Select(x =>
                {
                    var t = x.StartedAt ?? x.CreatedAt ?? DateTime.MinValue;
                    var hour = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
                    return new { Row = x, Started = t, Hour = hour };
                })
                .Where(x => x.Started != DateTime.MinValue)
                .ToList();

            // Build GLOBAL shaft sequential index (unique per physical shaft Guid, ordered by first StartedAt globally)
            var globalShaftMap = normalized
                .Where(x => x.Row.ShaftNum.HasValue)
                .GroupBy(x => x.Row.ShaftNum!.Value)
                .Select(g => new { ShaftGuid = g.Key, MinStarted = g.Min(x => x.Started) })
                .OrderBy(x => x.MinStarted)
                .Select((x, idx) => new { x.ShaftGuid, GlobalNo = idx + 1 })
                .ToDictionary(k => k.ShaftGuid, v => v.GlobalNo);

            int ResolveGlobalShaftNo(Guid? shaftNum)
            {
                if (!shaftNum.HasValue) return 0;
                return globalShaftMap.TryGetValue(shaftNum.Value, out var no) ? no : 0;
            }

            _stepRows = normalized
                .OrderBy(x => x.Started)
                .Select(x =>
                {
                    var durationText = "N/A";
                    if (x.Row.StartedAt.HasValue && x.Row.EndedAt.HasValue)
                    {
                        var dur = x.Row.EndedAt.Value - x.Row.StartedAt.Value;
                        durationText = $"{(int)dur.TotalHours:D2}:{dur.Minutes:D2}:{dur.Seconds:D2}";
                    }
                    else if (x.Row.StartedAt.HasValue && !x.Row.EndedAt.HasValue)
                    {
                        durationText = "Đang chạy...";
                    }

                    var stepName = string.IsNullOrWhiteSpace(x.Row.StepName) ? "N/A" : x.Row.StepName;
                    var stepIdText = x.Row.StepId.HasValue ? x.Row.StepId.Value.ToString() : "N/A";
                    var globalNo = ResolveGlobalShaftNo(x.Row.ShaftNum);

                    return new RevoStepRow
                    {
                        RevoName = x.Row.RevoName,
                        Hour = x.Hour,
                        HourBucket = $"{x.Hour:HH}:00-{x.Hour.AddHours(1):HH}:00",
                        ShaftNo = globalNo,
                        ShaftKey = globalNo > 0 ? $"Shaft {globalNo}" : "(Không xác định)",
                        ShaftNum = x.Row.ShaftNum,
                        Part = x.Row.Part,
                        Work = x.Row.Work,
                        Rev = x.Row.Rev,
                        Mandrel = x.Row.Mandrel,
                        StepDisplay = $"{stepName} ({stepIdText})",
                        StartedAt = x.Row.StartedAt,
                        EndedAt = x.Row.EndedAt,
                        DurationText = durationText,
                        Stt = 0
                    };
                })
                .ToList();

            // STT reset theo từng shaft (dùng ShaftNo global)
            var sttByShaft = new Dictionary<int, int>();
            foreach (var row in _stepRows)
            {
                if (!sttByShaft.ContainsKey(row.ShaftNo))
                    sttByShaft[row.ShaftNo] = 0;
                sttByShaft[row.ShaftNo] += 1;
                row.Stt = sttByShaft[row.ShaftNo];
            }

            // By shaft - 1 dòng / 1 ShaftNum (tổng hợp)
            var shaftGroups = normalized
                .Where(x => x.Row.ShaftNum.HasValue)
                .GroupBy(x => x.Row.ShaftNum!.Value)
                .Select(g =>
                {
                    var first = g.OrderBy(x => x.Started).First().Row;
                    var start = g.Min(x => x.Row.StartedAt) ?? g.Min(x => x.Started);
                    var end = g.Max(x => x.Row.EndedAt) ?? (DateTime?)null;
                    var endFallback = end ?? start;
                    var totalTime = endFallback - start;

                    return new
                    {
                        ShaftNum = g.Key,
                        FirstRow = first,
                        StartedAt = start,
                        EndedAt = end,
                        TotalTime = totalTime,
                        StepCount = g.Count()
                    };
                })
                .OrderBy(x => x.StartedAt)
                .ToList();

            _shaftRows = new List<RevoShaftRow>();
            var shaftIndex = 0;
            foreach (var g in shaftGroups)
            {
                shaftIndex++;
                _shaftRows.Add(new RevoShaftRow
                {
                    ShaftLabel = $"Shaft {shaftIndex}",
                    RevoName = g.FirstRow.RevoName,
                    Hour = g.StartedAt,
                    HourBucket = $"{g.StartedAt:HH}:00-{g.StartedAt.AddHours(1):HH}:00",
                    ShaftNo = shaftIndex,
                    Stt = shaftIndex,
                    ShaftNum = g.ShaftNum,
                    Part = g.FirstRow.Part,
                    Work = g.FirstRow.Work,
                    Mandrel = g.FirstRow.Mandrel,
                    StartedAt = g.StartedAt,
                    EndedAt = g.EndedAt,
                    StepCount = g.StepCount,
                    TotalTime = g.TotalTime
                });
            }

            // By hour
            _hourRows = normalized
                .GroupBy(x => x.Hour)
                .Select(g =>
                {
                    var start = g.Min(x => x.Row.StartedAt) ?? g.Min(x => x.Started);
                    var end = g.Max(x => x.Row.EndedAt) ?? (DateTime?)null;
                    var endFallback = end ?? start;
                    var totalHours = (endFallback - start).TotalHours;
                    var shaftCount = g.Select(x => x.Row.ShaftNum).Where(x => x.HasValue).Select(x => x!.Value).Distinct().Count();

                    return new RevoHourRow
                    {
                        Hour = g.Key,
                        HourRange = $"{g.Key:dd/MM/yyyy HH}:00-{g.Key.AddHours(1):HH}:00",
                        ShaftCount = shaftCount,
                        StartedAt = start,
                        EndedAt = end,
                        TotalHours = totalHours
                    };
                })
                .OrderBy(x => x.StartedAt ?? DateTime.MinValue)
                .ToList();
        }

        public enum RevoReportMode
        {
            ByStep = 0,
            ByShaft = 1,
            ByHour = 2
        }

        public class ReportModeOption
        {
            public string Name { get; set; } = string.Empty;
            public RevoReportMode Value { get; set; }
        }

        public class RevoStepRow
        {
            public string? RevoName { get; set; }
            public DateTime Hour { get; set; }
            public string HourBucket { get; set; } = string.Empty;
            public int ShaftNo { get; set; }        // global sequential index
            public string ShaftKey { get; set; } = string.Empty; // "Shaft N" - dùng để group
            public int Stt { get; set; }
            public Guid? ShaftNum { get; set; }
            public string? Part { get; set; }
            public string? Work { get; set; }
            public string? Rev { get; set; }
            public string? Mandrel { get; set; }
            public string StepDisplay { get; set; } = string.Empty;
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
            public string DurationText { get; set; } = string.Empty;
        }

        public class RevoShaftRow
        {
            public string ShaftLabel { get; set; } = string.Empty; // "Shaft N" - tên tự đặt
            public string? RevoName { get; set; }                   // giữ lại để tham chiếu nếu cần
            public DateTime Hour { get; set; }
            public string HourBucket { get; set; } = string.Empty;
            public int ShaftNo { get; set; }
            public int Stt { get; set; }
            public Guid ShaftNum { get; set; }
            public string? Part { get; set; }
            public string? Work { get; set; }
            public string? Mandrel { get; set; }
            public int StepCount { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
            public TimeSpan TotalTime { get; set; }
            public string TotalTimeText => $"{(int)TotalTime.TotalHours:D2}:{TotalTime.Minutes:D2}:{TotalTime.Seconds:D2}";
        }

        public class RevoHourRow
        {
            public DateTime Hour { get; set; }
            public string HourRange { get; set; } = string.Empty;
            public int ShaftCount { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
            public double TotalHours { get; set; }
            public string TotalHoursText => $"{(int)TotalHours:D2}:{(int)(TotalHours % 1 * 60):D2}:{(int)(TotalHours % 1 * 3600 % 60):D2}";
        }

        public class RevoDropdownModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
