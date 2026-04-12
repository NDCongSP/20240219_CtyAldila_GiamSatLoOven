using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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
        /// <summary>Số bản ghi hiển thị theo phạm vi (đã hoàn thành = chỉ dòng thuộc shaft đủ TotalTime).</summary>
        private int _scopeRecordCount;
        private bool _reportGridsFromDatabase = false;
        /// <summary>Chế độ shaft: tất cả hoặc chỉ shaft đã hoàn thành (mọi bản ghi TotalTime lớn hơn 0).</summary>
        private RevoShaftScopeKind _shaftScope = RevoShaftScopeKind.Total;
        /// <summary>Đồng bộ với RadzenSwitch — true = chỉ shaft đã hoàn thành.</summary>
        private bool _shaftFinishedSwitch;

        private static readonly JsonSerializerOptions _jsonReadOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>Luôn theo switch — tránh lệch với _shaftScope khi chưa đồng bộ.</summary>
        private string ShaftScopeForApi => _shaftFinishedSwitch ? "finished" : "total";

        /// <summary>Đồng bộ enum với switch (gọi trước khi lọc client / gọi API).</summary>
        private void SyncShaftScopeFromSwitch()
        {
            _shaftScope = _shaftFinishedSwitch ? RevoShaftScopeKind.Finished : RevoShaftScopeKind.Total;
        }

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
                SyncShaftScopeFromSwitch();

                // Use NSwag client for FT09 GetFilter
                var filterModel = new ApiClient.RevoFilterModel
                {
                    GetAll = !_selectedRevoId.HasValue,
                    RevoId = _selectedRevoId,
                    FromDate = _fromDate,
                    ToDate = _toDate,
                    ShaftScope = ShaftScopeForApi
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
                        .OrderBy(x => x.ShaftNum ?? Guid.Empty)
                        .ThenBy(x => x.StepId ?? 0)
                        .ThenBy(x => x.StartedAt ?? x.CreatedAt ?? DateTime.MinValue)
                        .ToList();

                    await TryPopulateReportGridsFromDatabaseAsync(filterModel);
                    _hasData = _rawData.Any();
                    // Đếm shaft + bản ghi theo phạm vi (đồng bộ switch; GetFilter không lọc theo shaft)
                    UpdateScopeSummaryCounts();
                    _gridInitialized = false; // Reset to apply grouping on next render (step mode)
                }
                else
                {
                    _rawData = new List<FT09_RevoDatalog>();
                    _stepRows = new();
                    _shaftRows = new();
                    _hourRows = new();
                    _reportGridsFromDatabase = false;
                    _hasData = false;
                    _shaftCount = 0;
                    _scopeRecordCount = 0;
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
                _shaftCount = 0;
                _scopeRecordCount = 0;
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
            _shaftScope = RevoShaftScopeKind.Total;
            _shaftFinishedSwitch = false;
            _hasData = false;
            _shaftCount = 0;
            _scopeRecordCount = 0;
            _rawData.Clear();
            _stepRows.Clear();
            _shaftRows.Clear();
            _hourRows.Clear();
            _reportGridsFromDatabase = false;
            StateHasChanged();
        }

        private void OnModeChanged(object value)
        {
            // Radzen passes selected value (enum)
            _gridInitialized = false;
            if (_rawData.Count > 0 && !_reportGridsFromDatabase)
            {
                RebuildView();
            }
            StateHasChanged();
        }

        private async Task TryPopulateReportGridsFromDatabaseAsync(ApiClient.RevoFilterModel filterModel)
        {
            _reportGridsFromDatabase = false;
            _stepRows.Clear();
            _shaftRows.Clear();
            _hourRows.Clear();

            try
            {
                var http = HttpClientFactory.CreateClient("GiamSatAPI");
                var ok = _reportMode switch
                {
                    RevoReportMode.ByStep => await TryLoadStepViewAsync(http, filterModel),
                    RevoReportMode.ByShaft => await TryLoadShaftViewAsync(http, filterModel),
                    RevoReportMode.ByHour => await TryLoadHourViewAsync(http, filterModel),
                    _ => false
                };

                if (ok)
                {
                    _reportGridsFromDatabase = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Báo cáo", $"Lỗi gọi view SQL: {ex.Message}. Hiển thị theo tổng hợp trên client.");
            }

            RebuildView();
        }

        private void NotifyReportViewFallback(string? detail = null)
        {
            var err = string.IsNullOrWhiteSpace(detail) ? "Không tải được view báo cáo từ SQL" : detail;
            _notificationService.Notify(NotificationSeverity.Warning, "Báo cáo", $"{err}. Hiển thị theo tổng hợp trên client.");
        }

        private async Task<bool> TryLoadStepViewAsync(HttpClient http, ApiClient.RevoFilterModel filterModel)
        {
            var res = await PostReportResultAsync<List<RevoReportStepVm>>(http, "api/FT09/GetReportStepView", filterModel);
            if (res?.Succeeded == true && res.Data != null)
            {
                _stepRows = MapStepRows(res.Data);
                return true;
            }

            NotifyReportViewFallback(res?.Messages?.FirstOrDefault());
            return false;
        }

        private async Task<bool> TryLoadShaftViewAsync(HttpClient http, ApiClient.RevoFilterModel filterModel)
        {
            var res = await PostReportResultAsync<List<RevoReportShaftVm>>(http, "api/FT09/GetReportShaftView", filterModel);
            if (res?.Succeeded == true && res.Data != null)
            {
                _shaftRows = MapShaftRows(res.Data);
                return true;
            }

            NotifyReportViewFallback(res?.Messages?.FirstOrDefault());
            return false;
        }

        private async Task<bool> TryLoadHourViewAsync(HttpClient http, ApiClient.RevoFilterModel filterModel)
        {
            var res = await PostReportResultAsync<List<RevoReportHourVm>>(http, "api/FT09/GetReportHourView", filterModel);
            if (res?.Succeeded == true && res.Data != null)
            {
                _hourRows = MapHourRows(res.Data);
                return true;
            }

            NotifyReportViewFallback(res?.Messages?.FirstOrDefault());
            return false;
        }

        private static async Task<Result<T>?> PostReportResultAsync<T>(HttpClient http, string relativeUrl, ApiClient.RevoFilterModel body)
        {
            var response = await http.PostAsJsonAsync(relativeUrl, body);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<Result<T>>(_jsonReadOptions);
        }

        private static List<RevoStepRow> MapStepRows(IList<RevoReportStepVm> data)
        {
            return data.Select(v => new RevoStepRow
            {
                RevoName = v.RevoName,
                Hour = v.Hour,
                HourBucket = v.HourBucketDisplay ?? string.Empty,
                ShaftNo = (int)v.ShaftNo,
                ShaftKey = v.ShaftKey ?? string.Empty,
                Stt = (int)v.Stt,
                ShaftNum = v.ShaftNum,
                Part = v.Part,
                Work = v.Work,
                Rev = v.Rev,
                Mandrel = v.Mandrel,
                StepDisplay = v.StepDisplay ?? string.Empty,
                StartedAt = v.DisplayStartedAt,
                EndedAt = v.DisplayEndedAt,
                DurationText = v.DurationText ?? "N/A",
                IsAutoRolling = v.IsAutoRolling != 0,
                HighlightIncomplete = v.HighlightIncomplete
            }).ToList();
        }

        private static List<RevoShaftRow> MapShaftRows(IList<RevoReportShaftVm> data)
        {
            return data.Select(v => new RevoShaftRow
            {
                ShaftLabel = v.ShaftLabel ?? string.Empty,
                RevoName = v.RevoName,
                Hour = v.Hour,
                HourBucket = v.HourBucket ?? string.Empty,
                ShaftNo = (int)v.ShaftNo,
                Stt = (int)v.Stt,
                ShaftNum = v.ShaftNum,
                Part = v.Part,
                Work = v.Work,
                Mandrel = v.Mandrel,
                StepCount = (int)Math.Min(int.MaxValue, v.StepCount),
                StartedAt = v.StartedAt,
                EndedAt = v.EndedAt,
                TotalTime = TimeSpan.FromSeconds(Math.Max(0, v.TotalTimeSeconds)),
                HighlightIncomplete = v.HighlightIncomplete,
                IsShaftFinished = v.IsShaftFinished
            }).ToList();
        }

        private static List<RevoHourRow> MapHourRows(IList<RevoReportHourVm> data)
        {
            return data.Select(v => new RevoHourRow
            {
                Hour = v.Hour,
                HourRange = v.HourRange ?? string.Empty,
                ShaftCount = v.ShaftCount,
                ShaftCountFinishedInHour = v.ShaftCountFinishedInHour,
                IncompleteShaftCountInHour = v.IncompleteShaftCountInHour,
                StartedAt = v.StartedAt,
                EndedAt = v.EndedAt,
                TotalTime = TimeSpan.FromSeconds(Math.Max(0, v.TotalTimeSeconds)),
                HighlightIncomplete = v.HighlightIncomplete
            }).ToList();
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
                var excelBytes = excel.GenerateExcelFileAsync(
                    _rawData,
                    $"{_fromDate:dd/MM/yyyy} đến {_toDate:dd/MM/yyyy}",
                    _reportMode,
                    _shaftFinishedSwitch ? RevoShaftScopeKind.Finished : RevoShaftScopeKind.Total,
                    _reportGridsFromDatabase,
                    _stepRows,
                    _shaftRows,
                    _hourRows);
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

        /// <summary>Class + CSS global (app.css) — Radzen ghi nền lên td nên inline style thường không thấy.</summary>
        private const string WarningRowClass = "revo-shaft-warn-row";

        private static void AppendRowWarningClass(IDictionary<string, object> attrs)
        {
            if (attrs.TryGetValue("class", out var c) && c is string s && !string.IsNullOrEmpty(s))
                attrs["class"] = $"{s} {WarningRowClass}".Trim();
            else
                attrs["class"] = WarningRowClass;
        }

        private void OnStepRowRender(RowRenderEventArgs<RevoStepRow> args)
        {
            if (args.Data is { HighlightIncomplete: true })
                AppendRowWarningClass(args.Attributes);
        }

        private void OnShaftRowRender(RowRenderEventArgs<RevoShaftRow> args)
        {
            if (args.Data is { HighlightIncomplete: true })
                AppendRowWarningClass(args.Attributes);
        }

        private void OnHourRowRender(RowRenderEventArgs<RevoHourRow> args)
        {
            if (args.Data is { HighlightIncomplete: true })
                AppendRowWarningClass(args.Attributes);
        }

        /// <summary>Sau @bind-Value Radzen đã cập nhật _shaftFinishedSwitch; chỉ đồng bộ enum và tải lại.</summary>
        private async Task OnShaftScopeSwitchChange(bool _)
        {
            SyncShaftScopeFromSwitch();
            if (_hasData)
                await ExecuteSearch();
        }

        /// <summary>Tập shaft "hoàn thành" trong phạm vi dữ liệu đã tải (mọi dòng có TotalTime lớn hơn 0).</summary>
        private static HashSet<Guid> BuildFinishedShaftSet(IEnumerable<FT09_RevoDatalog> rows)
        {
            var set = new HashSet<Guid>();
            foreach (var g in rows.Where(r => r != null && r.ShaftNum.HasValue).GroupBy(r => r.ShaftNum!.Value))
            {
                if (g.All(r => (r.TotalTime ?? 0) > 0))
                    set.Add(g.Key);
            }
            return set;
        }

        private static int CountFinishedShafts(IEnumerable<FT09_RevoDatalog> rows) => BuildFinishedShaftSet(rows).Count;

        /// <summary>Số dòng FT09 thuộc phạm vi "đã hoàn thành" (khớp WHERE TVF: không shaft hoặc shaft finished).</summary>
        private static int CountRecordsInFinishedShaftScope(IReadOnlyList<FT09_RevoDatalog> raw)
        {
            if (raw.Count == 0) return 0;
            var finished = BuildFinishedShaftSet(raw);
            return raw.Count(r => r.ShaftNum is null || finished.Contains(r.ShaftNum.Value));
        }

        /// <summary>Cập nhật _shaftCount và _scopeRecordCount sau khi có _rawData (phụ thuộc switch phạm vi).</summary>
        private void UpdateScopeSummaryCounts()
        {
            var finishedMode = _shaftFinishedSwitch;
            _shaftCount = finishedMode
                ? CountFinishedShafts(_rawData)
                : _rawData.Where(x => x.ShaftNum.HasValue).Select(x => x.ShaftNum!.Value).Distinct().Count();
            _scopeRecordCount = finishedMode
                ? CountRecordsInFinishedShaftScope(_rawData)
                : _rawData.Count;
        }

        private void RebuildView()
        {
            SyncShaftScopeFromSwitch();
            var finishedShafts = BuildFinishedShaftSet(_rawData);

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

            if (_shaftScope == RevoShaftScopeKind.Finished)
            {
                normalized = normalized
                    .Where(x => !x.Row.ShaftNum.HasValue || finishedShafts.Contains(x.Row.ShaftNum.Value))
                    .ToList();
            }

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
                    var isAutoRolling = IsAutoRolling(x.Row);
                    var durationText = "N/A";
                    if (isAutoRolling)
                    {
                        var total = x.Row.TotalTime ?? 0;
                        durationText = total > 0 ? FormatDuration(TimeSpan.FromSeconds(total)) : "N/A";
                    }
                    else if (x.Row.StartedAt.HasValue && x.Row.EndedAt.HasValue)
                    {
                        var dur = x.Row.EndedAt.Value - x.Row.StartedAt.Value;
                        durationText = FormatDuration(dur);
                    }
                    else if (x.Row.StartedAt.HasValue && !x.Row.EndedAt.HasValue)
                    {
                        durationText = "Đang chạy...";
                    }

                    var stepName = string.IsNullOrWhiteSpace(x.Row.StepName) ? "N/A" : x.Row.StepName;
                    var stepIdText = x.Row.StepId.HasValue ? x.Row.StepId.Value.ToString() : "N/A";
                    var globalNo = ResolveGlobalShaftNo(x.Row.ShaftNum);

                    var inc = _shaftScope == RevoShaftScopeKind.Total
                        && x.Row.ShaftNum.HasValue
                        && !finishedShafts.Contains(x.Row.ShaftNum.Value);

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
                        StartedAt = isAutoRolling ? null : x.Row.StartedAt,
                        EndedAt = isAutoRolling ? null : x.Row.EndedAt,
                        DurationText = durationText,
                        IsAutoRolling = isAutoRolling,
                        Stt = 0,
                        HighlightIncomplete = inc
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
            // Non auto rolling: min StartedAt, max EndedAt, Thời lượng = sum(Row.TotalTime). Không filter theo null.
            // Auto rolling: bỏ qua StartedAt/EndedAt (hiển thị N/A), Thời lượng = sum(Row.TotalTime).
            var shaftGroups = normalized
                .Where(x => x.Row.ShaftNum.HasValue)
                .GroupBy(x => x.Row.ShaftNum!.Value)
                .Select(g =>
                {
                    var first = g.OrderBy(x => x.Started).First().Row;
                    var isAutoRolling = g.Any(x => IsAutoRolling(x.Row));
                    var totalSeconds = g.Sum(x => x.Row.TotalTime ?? 0);
                    var totalTime = totalSeconds > 0 ? TimeSpan.FromSeconds(totalSeconds) : TimeSpan.Zero;
                    DateTime orderKey;

                    if (isAutoRolling)
                    {
                        orderKey = g.Min(x => x.Started);
                        return new
                        {
                            ShaftNum   = g.Key,
                            FirstRow   = first,
                            StartedAt  = (DateTime?)null,
                            EndedAt    = (DateTime?)null,
                            TotalTime  = totalTime,
                            StepCount  = g.Count(),
                            OrderKey   = orderKey
                        };
                    }

                    var start = g.Min(x => x.Row.StartedAt);
                    var end   = g.Max(x => x.Row.EndedAt);
                    orderKey = start ?? g.Min(x => x.Started);
                    return new
                    {
                        ShaftNum   = g.Key,
                        FirstRow   = first,
                        StartedAt  = start,
                        EndedAt    = end,
                        TotalTime  = totalTime,
                        StepCount  = g.Count(),
                        OrderKey   = orderKey
                    };
                })
                .OrderBy(x => x.OrderKey)
                .ToList();

            _shaftRows = new List<RevoShaftRow>();
            var shaftIndex = 0;
            foreach (var g in shaftGroups)
            {
                shaftIndex++;
                var hourForBucket = g.StartedAt ?? g.OrderKey;
                var shaftFinished = finishedShafts.Contains(g.ShaftNum);
                _shaftRows.Add(new RevoShaftRow
                {
                    ShaftLabel = $"Shaft {shaftIndex}",
                    RevoName = g.FirstRow.RevoName,
                    Hour = hourForBucket,
                    HourBucket = $"{hourForBucket:HH}:00-{hourForBucket.AddHours(1):HH}:00",
                    ShaftNo = shaftIndex,
                    Stt = shaftIndex,
                    ShaftNum = g.ShaftNum,
                    Part = g.FirstRow.Part,
                    Work = g.FirstRow.Work,
                    Mandrel = g.FirstRow.Mandrel,
                    StartedAt = g.StartedAt,
                    EndedAt = g.EndedAt,
                    StepCount = g.StepCount,
                    TotalTime = g.TotalTime,
                    IsShaftFinished = shaftFinished,
                    HighlightIncomplete = _shaftScope == RevoShaftScopeKind.Total && !shaftFinished
                });
            }

            // By hour
            _hourRows = normalized
                .GroupBy(x => x.Hour)
                .Select(g =>
                {
                    var autoRows = g.Where(x => IsAutoRolling(x.Row)).ToList();
                    var nonAutoRows = g.Where(x => !IsAutoRolling(x.Row)).ToList();
                    var isAutoOnly = nonAutoRows.Count == 0 && autoRows.Count > 0;

                    var start = isAutoOnly
                        ? (DateTime?)null
                        : nonAutoRows.Select(x => x.Row.StartedAt).Where(x => x.HasValue).DefaultIfEmpty(g.Min(x => x.Started)).Min();

                    var end = isAutoOnly
                        ? (DateTime?)null
                        : nonAutoRows.Select(x => x.Row.EndedAt).Where(x => x.HasValue).DefaultIfEmpty(null).Max();

                    var totalSeconds = g.Sum(x => x.Row.TotalTime ?? 0);
                    var totalTime = totalSeconds > 0
                        ? TimeSpan.FromSeconds(totalSeconds)
                        : TimeSpan.Zero;
                    var shaftCount = g.Select(x => x.Row.ShaftNum).Where(x => x.HasValue).Select(x => x!.Value).Distinct().Count();
                    var distinctInHour = g.Where(x => x.Row.ShaftNum.HasValue).Select(x => x.Row.ShaftNum!.Value).Distinct().ToList();
                    var finishedInHour = distinctInHour.Count(sn =>
                        g.Where(x => x.Row.ShaftNum == sn).All(x => (x.Row.TotalTime ?? 0) > 0));

                    return new RevoHourRow
                    {
                        Hour = g.Key,
                        HourRange = $"{g.Key:dd/MM/yyyy HH}:00-{g.Key.AddHours(1):HH}:00",
                        ShaftCount = shaftCount,
                        ShaftCountFinishedInHour = finishedInHour,
                        IncompleteShaftCountInHour = shaftCount - finishedInHour,
                        StartedAt = start,
                        EndedAt = end,
                        TotalTime = totalTime,
                        HighlightIncomplete = _shaftScope == RevoShaftScopeKind.Total && finishedInHour < shaftCount
                    };
                })
                .OrderBy(x => x.StartedAt ?? DateTime.MinValue)
                .ToList();
        }

        private static bool IsAutoRolling(FT09_RevoDatalog row)
        {
            var revo = row.RevoName ?? string.Empty;
            var work = row.Work ?? string.Empty;
            return revo.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || work.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || revo.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase)
                || work.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatDuration(TimeSpan ts)
            => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

        public enum RevoReportMode
        {
            ByStep = 0,
            ByShaft = 1,
            ByHour = 2
        }

        /// <summary>Tổng hợp tất cả shaft trong kỳ vs chỉ shaft đã hoàn thành (TVF / API).</summary>
        public enum RevoShaftScopeKind
        {
            Total = 0,
            Finished = 1
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
            public bool IsAutoRolling { get; set; }
            /// <summary>True khi chế độ "Tất cả shaft" và shaft chưa đủ TotalTime — tô nền cảnh báo.</summary>
            public bool HighlightIncomplete { get; set; }
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
            public bool IsShaftFinished { get; set; }
            public bool HighlightIncomplete { get; set; }
        }

        public class RevoHourRow
        {
            public DateTime Hour { get; set; }
            public string HourRange { get; set; } = string.Empty;
            public int ShaftCount { get; set; }
            public long ShaftCountFinishedInHour { get; set; }
            public long IncompleteShaftCountInHour { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
            public TimeSpan TotalTime { get; set; }
            public string TotalHoursText => $"{(int)TotalTime.TotalHours:D2}:{TotalTime.Minutes:D2}:{TotalTime.Seconds:D2}";
            public bool HighlightIncomplete { get; set; }
        }

        public class RevoDropdownModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
