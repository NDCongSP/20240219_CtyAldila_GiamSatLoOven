using ClosedXML.Excel;
using GiamSat.APIClient;
using GiamSat.Models;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using System.IO;
using FT14_TipOdFreq      = GiamSat.APIClient.FT14_TipOdFreq;
using AutoSandingTestRow  = GiamSat.Models.AutoSandingTestRow;

namespace GiamSat.UI.Pages
{
    public partial class AutoSandingConfig
    {
        // Ép RadzenNumeric dùng InvariantCulture ("." là dấu thập phân) để tránh lỗi
        // Chrome theo locale máy (vd dấu phẩy) làm "280.56" bị parse thành 28056.
        private readonly System.Globalization.CultureInfo _culture = System.Globalization.CultureInfo.InvariantCulture;

        // ── State ────────────────────────────────────────────────────────────
        private int  _selectedTab    = 0;
        private bool _isLoading      = true;
        private bool _isSaving       = false;
        private bool _isLoadingCalc  = false;
        private bool _isExporting    = false;
        private bool _isImporting    = false;

        // ── Đồng bộ Part từ external DB ALD_MFG → FT14 ───────────────────────
        private bool   _isSyncing      = false;
        private double _syncPercent    = 0;
        private int    _syncProcessed  = 0;
        private int    _syncTotal      = 0;
        private string _syncStatus     = string.Empty;
        private SyncAccumulator _syncResult = new();
        private bool _abcdCalculated = false;
        private bool _showGuide      = false;

        // Tab 1 – danh sách part từ DB
        private List<FT14_TipOdFreq> _parts = new();

        // Tab 2 – tính ABCD
        private System.Guid? _selectedPartId = null;
        private List<AutoSandingTestRow> _testRows = new();
        private LinearRegressionResult _resultAB = new();
        private LinearRegressionResult _resultCD = new();

        // Tab 2 – form load data từ external DB
        private string  _workFre1     = string.Empty;
        private string  _workFre2     = string.Empty;
        private string  _workSpine    = string.Empty;

        // Tab 2 – danh sách Work theo Part (dropdown)
        private List<string> _freWorks   = new();   // Fre1 (toàn bộ work của part)
        private List<string> _freWorks2  = new();   // Fre2 = _freWorks trừ work đã chọn ở Fre1
        private List<string> _spineWorks = new();   // Spine (FT16 Test)
        private bool _worksLoading = false;
        private double  _offsetFre1   = 0;
        private double  _offsetFre2   = 0;
        private double  _offsetSpine  = 0;
        private int     _formular     = 1;
        private double  _motorFrom    = 100;
        private double  _motorTo      = 500;
        private double  _motorStep    = 100;

        // Chart data
        private List<ChartPoint> _chartABPoints = new();
        private List<ChartPoint> _trendABPoints = new();
        private List<ChartPoint> _chartCDPoints = new();
        private List<ChartPoint> _trendCDPoints = new();

        // Ref tới bảng nhập liệu test để gắn điều hướng Enter (focus ô dưới cùng cột)
        private ElementReference _testTableRef;

        // JS điều hướng Enter — định nghĩa qua eval (không phụ thuộc file tĩnh bị cache),
        // tránh lỗi "setupEnterNav undefined" + debugger break. Bind 1 lần/element.
        private const string EnterNavScript = @"
window.setupEnterNav = function (tableEl) {
    if (!tableEl || tableEl._enterNavBound) return;
    tableEl._enterNavBound = true;
    tableEl.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        var input = e.target;
        if (!input || input.tagName !== 'INPUT') return;
        var td = input.closest('td');
        var tr = input.closest('tr');
        if (!td || !tr) return;
        e.preventDefault();
        var colIndex = td.cellIndex;
        var nextTr = tr.nextElementSibling;
        while (nextTr && nextTr.tagName !== 'TR') nextTr = nextTr.nextElementSibling;
        if (!nextTr) { input.blur(); return; }
        var nextTd = nextTr.cells ? nextTr.cells[colIndex] : null;
        if (!nextTd) return;
        var nextInput = nextTd.querySelector('input');
        if (nextInput) { nextInput.focus(); if (nextInput.select) nextInput.select(); }
    });
};";

        // ── Lifecycle ────────────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            // Enter trong bảng test → nhảy ô dưới cùng cột. Định nghĩa hàm rồi gắn 1 lần.
            try
            {
                await _js.InvokeVoidAsync("eval", EnterNavScript);
                await _js.InvokeVoidAsync("setupEnterNav", _testTableRef);
            }
            catch { /* bỏ qua nếu JS chưa sẵn sàng */ }
        }

        // ── Tải dữ liệu từ DB ────────────────────────────────────────────────
        private async Task LoadData()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                var result = await _fT14Client.GetAllAsync();
                if (result.Succeeded && result.Data != null)
                    _parts = result.Data.Where(x => x.Actived != false).OrderBy(x => x.PartName).ToList();
                else
                {
                    _parts = new List<FT14_TipOdFreq>();
                    var msg = result?.Messages != null ? string.Join(", ", result.Messages) : "API trả về lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi tải dữ liệu FT14", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Không tải được dữ liệu: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        // ── Tab 1 – CRUD Part ────────────────────────────────────────────────
        private async Task OnAddPart()
        {
            var blank = new FT14_TipOdFreq
            {
                Id      = System.Guid.Empty,
                Actived = true,
                // Formula để null như A,B,C,D — người dùng tự nhập
            };

            var saved = await OpenPartDialog("Thêm Part mới", blank, isEdit: false);
            if (saved == null) return;

            try
            {
                saved.Id        = System.Guid.NewGuid();
                saved.CreatedAt = DateTime.Now;
                saved.CreatedMachine = Environment.MachineName;
                saved.Actived   = true;

                var result = await _fT14Client.InsertAsync(saved);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Thêm thành công", $"Đã thêm part \"{saved.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
        }

        // ── Đồng bộ dữ liệu Part từ external DB ALD_MFG → FT14 ───────────────
        private async Task OnSyncData()
        {
            try
            {
                _isSyncing     = true;
                _syncPercent   = 0;
                _syncProcessed = 0;
                _syncTotal     = 0;
                _syncStatus    = "Đang lấy danh sách Part nguồn...";
                _syncResult    = new SyncAccumulator();
                StateHasChanged();

                var sourcesRes = await _ft14SyncClient.GetSyncSourcesAsync();
                if (sourcesRes == null || !sourcesRes.Succeeded || sourcesRes.Data == null)
                {
                    var m = sourcesRes?.Messages != null ? string.Join(", ", sourcesRes.Messages) : "Không lấy được danh sách Part nguồn";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi đồng bộ", m);
                    return;
                }

                var ids = sourcesRes.Data.Select(s => s.PartId).Distinct().ToList();
                _syncTotal = ids.Count;
                if (_syncTotal == 0)
                {
                    _notificationService.Notify(NotificationSeverity.Info, "Đồng bộ", "Không có Part nào để đồng bộ.");
                    return;
                }

                _syncStatus = "Đang đồng bộ vào FT14...";
                var errorMessages = new HashSet<string>();
                const int batchSize = 50;
                for (int i = 0; i < ids.Count; i += batchSize)
                {
                    var batch = ids.Skip(i).Take(batchSize).ToList();
                    try
                    {
                        var res = await _ft14SyncClient.SyncPartsAsync(batch);
                        if (res != null && res.Succeeded && res.Data != null)
                        {
                            _syncResult.Inserted += res.Data.Inserted;
                            _syncResult.Updated  += res.Data.Updated;
                            _syncResult.Skipped  += res.Data.Skipped;
                            _syncResult.Failed   += res.Data.Failed;
                            if (res.Data.Messages != null)
                                foreach (var m in res.Data.Messages) errorMessages.Add(m);
                        }
                        else
                        {
                            _syncResult.Failed += batch.Count;
                            if (res?.Messages != null)
                                foreach (var m in res.Messages) errorMessages.Add(m);
                        }
                    }
                    catch (Exception exBatch)
                    {
                        _syncResult.Failed += batch.Count;
                        errorMessages.Add(exBatch.Message);
                    }

                    _syncProcessed = Math.Min(i + batch.Count, _syncTotal);
                    _syncPercent   = Math.Round((double)_syncProcessed / _syncTotal * 100, 1);
                    StateHasChanged();
                }

                await LoadData();

                if (_syncResult.Failed > 0)
                {
                    var detail = errorMessages.Count > 0 ? string.Join(" | ", errorMessages.Take(3)) : "Xem log API.";
                    _notificationService.Notify(NotificationSeverity.Warning, "Đồng bộ xong (có lỗi)",
                        $"Thêm {_syncResult.Inserted}, cập nhật {_syncResult.Updated}, bỏ qua {_syncResult.Skipped}, lỗi {_syncResult.Failed}. Lỗi: {detail}",
                        duration: 12000);
                }
                else
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Đồng bộ hoàn tất",
                        $"Thêm {_syncResult.Inserted}, cập nhật {_syncResult.Updated}, bỏ qua {_syncResult.Skipped}.");
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi đồng bộ", ex.Message);
            }
            finally
            {
                _isSyncing = false;
                StateHasChanged();
            }
        }

        // Tích lũy kết quả đồng bộ qua nhiều batch.
        private class SyncAccumulator
        {
            public int Inserted { get; set; }
            public int Updated  { get; set; }
            public int Skipped  { get; set; }
            public int Failed   { get; set; }
        }

        private async Task OnEditPart(FT14_TipOdFreq item)
        {
            var saved = await OpenPartDialog($"Chỉnh sửa – {item.PartName}", item, isEdit: true);
            if (saved == null) return;

            try
            {
                saved.UpdateddAt = DateTime.Now;
                var result = await _fT14Client.UpdateAsync(saved);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Cập nhật thành công", $"Đã lưu part \"{saved.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
        }

        private async Task OnDeletePart(FT14_TipOdFreq item)
        {
            var confirm = await _dialogService.Confirm(
                $"Bạn có chắc muốn xóa part \"{item.PartName}\"?",
                "Xác nhận xóa",
                new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });

            if (confirm != true) return;

            try
            {
                // Soft-delete: set Actived = false
                item.Actived    = false;
                item.UpdateddAt = DateTime.Now;
                var result = await _fT14Client.UpdateAsync(item);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Đã xóa", $"Đã xóa part \"{item.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
        }

        private async Task<FT14_TipOdFreq?> OpenPartDialog(string title, FT14_TipOdFreq model, bool isEdit)
        {
            var result = await _dialogService.OpenAsync<DialogAutoSandingConfig>(
                title,
                new Dictionary<string, object>
                {
                    { "Model",  model  },
                    { "IsEdit", isEdit }
                },
                new DialogOptions { Width = "720px", Resizable = true, Draggable = true });

            return result as FT14_TipOdFreq;
        }

        // ── Export / Import Excel ─────────────────────────────────────────────
        private async Task OnExportExcel()
        {
            try
            {
                _isExporting = true;
                StateHasChanged();

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("AutoSanding Config");

                var headers = new[]
                {
                    "Item Number","Length","OD/BOD","Freq Target","Freq LL","Freq UL","Formula",
                    "A","B","C","D","Z_Stiffness",
                    "Diam LL 1","Diam UL 1","Tip OD Length 1",
                    "Diam LL 2","Diam UL 2","Tip OD Length 2",
                    "Diam LL 3","Diam UL 3","Tip OD Length 3"
                };
                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(1, i + 1).Value = headers[i];

                var hdrRow = ws.Row(1);
                hdrRow.Style.Font.Bold = true;
                hdrRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

                int row = 2;
                foreach (var p in _parts)
                {
                    ws.Cell(row, 1).Value  = p.PartName ?? "";
                    ws.Cell(row, 2).Value  = p.Length ?? 0;
                    ws.Cell(row, 3).Value  = p.OD_BOD ?? 0;
                    ws.Cell(row, 4).Value  = p.FreqTarget ?? 0;
                    ws.Cell(row, 5).Value  = p.Freq_LL ?? 0;
                    ws.Cell(row, 6).Value  = p.Freq_UL ?? 0;
                    ws.Cell(row, 7).Value  = p.Formula ?? 0;
                    ws.Cell(row, 8).Value  = p.A ?? 0;
                    ws.Cell(row, 9).Value  = p.B ?? 0;
                    ws.Cell(row, 10).Value = p.C ?? 0;
                    ws.Cell(row, 11).Value = p.D ?? 0;
                    if (p.Z_Stiffness.HasValue) ws.Cell(row, 12).Value = p.Z_Stiffness.Value;
                    ws.Cell(row, 13).Value = p.Diam_LL_1 ?? 0;
                    ws.Cell(row, 14).Value = p.Diam_UL_1 ?? 0;
                    ws.Cell(row, 15).Value = p.TipOdLength_1 ?? "";
                    ws.Cell(row, 16).Value = p.Diam_LL_2 ?? 0;
                    ws.Cell(row, 17).Value = p.Diam_UL_2 ?? 0;
                    ws.Cell(row, 18).Value = p.TipOdLength_2 ?? "";
                    ws.Cell(row, 19).Value = p.Diam_LL_3 ?? 0;
                    ws.Cell(row, 20).Value = p.Diam_UL_3 ?? 0;
                    ws.Cell(row, 21).Value = p.TipOdLength_3 ?? "";
                    row++;
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                await _js.InvokeVoidAsync("BlazorDownloadFile",
                    $"AutoSandingConfig_{DateTime.Now:yyyyMMdd_HHmm}.xlsx", ms.ToArray());
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi Export", ex.Message);
            }
            finally
            {
                _isExporting = false;
                StateHasChanged();
            }
        }

        private async Task OnImportClick()
        {
            await _js.InvokeVoidAsync("eval", "document.getElementById('ft14ImportInput').click()");
        }

        private async Task OnImportFileSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;

            try
            {
                _isImporting = true;
                StateHasChanged();

                using var ms = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).CopyToAsync(ms);
                ms.Position = 0;

                using var wb = new XLWorkbook(ms);
                var ws = wb.Worksheet(1);
                var dataRows = ws.RowsUsed().Skip(1);

                int inserted = 0, updated = 0, skipped = 0;
                foreach (var dataRow in dataRows)
                {
                    var partName = dataRow.Cell(1).GetString();
                    if (string.IsNullOrWhiteSpace(partName)) { skipped++; continue; }

                    var existing = _parts.FirstOrDefault(p => p.PartName == partName);
                    if (existing != null)
                    {
                        existing.Length      = dataRow.Cell(2).GetDouble();
                        existing.OD_BOD      = dataRow.Cell(3).GetDouble();
                        existing.FreqTarget  = dataRow.Cell(4).GetDouble();
                        existing.Freq_LL     = dataRow.Cell(5).GetDouble();
                        existing.Freq_UL     = dataRow.Cell(6).GetDouble();
                        existing.Formula     = dataRow.Cell(7).GetDouble();
                        existing.A           = dataRow.Cell(8).GetDouble();
                        existing.B           = dataRow.Cell(9).GetDouble();
                        existing.C           = dataRow.Cell(10).GetDouble();
                        existing.D           = dataRow.Cell(11).GetDouble();
                        var zStr = dataRow.Cell(12).GetString();
                        existing.Z_Stiffness = double.TryParse(zStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var zVal) ? zVal : (double?)null;
                        existing.Diam_LL_1   = dataRow.Cell(13).GetDouble();
                        existing.Diam_UL_1   = dataRow.Cell(14).GetDouble();
                        existing.TipOdLength_1 = dataRow.Cell(15).GetString();
                        existing.Diam_LL_2   = dataRow.Cell(16).GetDouble();
                        existing.Diam_UL_2   = dataRow.Cell(17).GetDouble();
                        existing.TipOdLength_2 = dataRow.Cell(18).GetString();
                        existing.Diam_LL_3   = dataRow.Cell(19).GetDouble();
                        existing.Diam_UL_3   = dataRow.Cell(20).GetDouble();
                        existing.TipOdLength_3 = dataRow.Cell(21).GetString();
                        existing.UpdateddAt            = DateTime.Now;
                        await _fT14Client.UpdateAsync(existing);
                        updated++;
                    }
                    else
                    {
                        var newPart = new FT14_TipOdFreq
                        {
                            Id                    = Guid.NewGuid(),
                            PartName              = partName,
                            CreatedAt             = DateTime.Now,
                            CreatedMachine        = Environment.MachineName,
                            Actived               = true,
                            Length        = dataRow.Cell(2).GetDouble(),
                            OD_BOD        = dataRow.Cell(3).GetDouble(),
                            FreqTarget    = dataRow.Cell(4).GetDouble(),
                            Freq_LL       = dataRow.Cell(5).GetDouble(),
                            Freq_UL       = dataRow.Cell(6).GetDouble(),
                            Formula       = dataRow.Cell(7).GetDouble(),
                            A             = dataRow.Cell(8).GetDouble(),
                            B             = dataRow.Cell(9).GetDouble(),
                            C             = dataRow.Cell(10).GetDouble(),
                            D             = dataRow.Cell(11).GetDouble(),
                            Z_Stiffness   = double.TryParse(dataRow.Cell(12).GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var zNew) ? zNew : (double?)null,
                            Diam_LL_1     = dataRow.Cell(13).GetDouble(),
                            Diam_UL_1     = dataRow.Cell(14).GetDouble(),
                            TipOdLength_1 = dataRow.Cell(15).GetString(),
                            Diam_LL_2     = dataRow.Cell(16).GetDouble(),
                            Diam_UL_2     = dataRow.Cell(17).GetDouble(),
                            TipOdLength_2 = dataRow.Cell(18).GetString(),
                            Diam_LL_3     = dataRow.Cell(19).GetDouble(),
                            Diam_UL_3     = dataRow.Cell(20).GetDouble(),
                            TipOdLength_3 = dataRow.Cell(21).GetString(),
                        };
                        await _fT14Client.InsertAsync(newPart);
                        inserted++;
                    }
                }

                _notificationService.Notify(NotificationSeverity.Success, "Import hoàn tất",
                    $"Thêm mới: {inserted} | Cập nhật: {updated} | Bỏ qua: {skipped}");
                await LoadData();
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi Import", ex.Message);
            }
            finally
            {
                _isImporting = false;
                StateHasChanged();
            }
        }

        // ── Tab 2 – Tính ABCD ────────────────────────────────────────────────
        private async Task OnPartSelected(object value)
        {
            _abcdCalculated = false;
            _testRows       = new List<AutoSandingTestRow>();

            // Reset lựa chọn work cũ
            _workFre1 = _workFre2 = _workSpine = string.Empty;
            _freWorks = new(); _freWorks2 = new(); _spineWorks = new();

            var part = _parts.FirstOrDefault(p => p.Id == _selectedPartId);
            if (part != null)
                _formular = (int)(part.Formula ?? 1);

            await LoadWorksForPart(part?.PartName);
            StateHasChanged();
        }

        // Load danh sách Work liên quan tới Part (Fre từ external DB, Spine từ FT16)
        private async Task LoadWorksForPart(string? partName)
        {
            if (string.IsNullOrWhiteSpace(partName)) return;
            try
            {
                _worksLoading = true;
                StateHasChanged();

                var res = await _ft14CalcDataClient.GetWorksAsync(partName);
                if (res != null && res.Succeeded && res.Data != null)
                {
                    _freWorks   = res.Data.FreWorks   ?? new();
                    _spineWorks = res.Data.SpineWorks ?? new();
                    _freWorks2  = new List<string>(_freWorks);
                }
                else
                {
                    var msg = res?.Messages != null ? string.Join(", ", res.Messages) : "Không tải được danh sách Work";
                    _notificationService.Notify(NotificationSeverity.Warning, "Danh sách Work", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi tải Work", ex.Message);
            }
            finally
            {
                _worksLoading = false;
                StateHasChanged();
            }
        }

        // Khi chọn Work Fre1 → mở Fre2; list Fre2 không chứa work đã chọn ở Fre1
        private void OnWorkFre1Changed(object value)
        {
            _freWorks2 = _freWorks.Where(w => w != _workFre1).ToList();
            if (_workFre2 == _workFre1) _workFre2 = string.Empty;
            StateHasChanged();
        }

        // Reset toàn bộ form Tab Tính ABCD về trạng thái ban đầu để chọn lại
        private void OnResetCalcForm()
        {
            _selectedPartId = null;
            _workFre1 = _workFre2 = _workSpine = string.Empty;
            _freWorks = new(); _freWorks2 = new(); _spineWorks = new();

            _offsetFre1 = _offsetFre2 = _offsetSpine = 0;
            _formular   = 1;
            _motorFrom  = 100; _motorTo = 500; _motorStep = 100;

            _testRows       = new List<AutoSandingTestRow>();
            _abcdCalculated = false;
            _resultAB = new LinearRegressionResult();
            _resultCD = new LinearRegressionResult();
            _chartABPoints = new(); _trendABPoints = new();
            _chartCDPoints = new(); _trendCDPoints = new();

            StateHasChanged();
        }

        private void OnAddTestRow()
        {
            _testRows.Add(new AutoSandingTestRow { RowIndex = _testRows.Count + 1 });
            StateHasChanged();
        }

        private void OnRemoveLastRow()
        {
            if (_testRows.Count > 0) { _testRows.RemoveAt(_testRows.Count - 1); StateHasChanged(); }
        }

        private void OnLoadFakeData()
        {
            _testRows = new List<AutoSandingTestRow>
            {
                new() { RowIndex=1,  Fre1=308.9, BeltRotationRpm=100, Fre2=308.1, StiffnessY=2.546 },
                new() { RowIndex=2,  Fre1=309.4, BeltRotationRpm=100, Fre2=308.6, StiffnessY=2.552 },
                new() { RowIndex=3,  Fre1=309.3, BeltRotationRpm=200, Fre2=306.3, StiffnessY=2.526 },
                new() { RowIndex=4,  Fre1=309.5, BeltRotationRpm=200, Fre2=306.8, StiffnessY=2.526 },
                new() { RowIndex=5,  Fre1=308.3, BeltRotationRpm=300, Fre2=303.2, StiffnessY=2.436 },
                new() { RowIndex=6,  Fre1=309.1, BeltRotationRpm=300, Fre2=302.9, StiffnessY=2.445 },
                new() { RowIndex=7,  Fre1=306.6, BeltRotationRpm=400, Fre2=300.0, StiffnessY=2.398 },
                new() { RowIndex=8,  Fre1=307.4, BeltRotationRpm=400, Fre2=299.4, StiffnessY=2.366 },
                new() { RowIndex=9,  Fre1=307.9, BeltRotationRpm=500, Fre2=298.3, StiffnessY=2.372 },
                new() { RowIndex=10, Fre1=307.8, BeltRotationRpm=500, Fre2=298.7, StiffnessY=2.364 },
            };
            _abcdCalculated = false;
            _notificationService.Notify(NotificationSeverity.Info, "Fake Data", "Đã nạp 10 dòng dữ liệu mẫu từ slide 14.");
            StateHasChanged();
        }

        private async Task OnLoadDataFromDB()
        {
            var partName = _parts.FirstOrDefault(p => p.Id == _selectedPartId)?.PartName;
            if (string.IsNullOrWhiteSpace(partName))
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Chưa chọn Part", "Vui lòng chọn Part trước khi load data.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_workFre1))
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Thiếu Work Fre1", "Vui lòng nhập Work Order cho Fre1.");
                return;
            }

            try
            {
                _isLoadingCalc = true;
                _abcdCalculated = false;
                StateHasChanged();

                var result = await _ft14CalcDataClient.GetCalcDataAsync(
                    partName, _workFre1, _workFre2, _workSpine,
                    _offsetFre1, _offsetFre2,
                    _motorFrom, _motorTo, _motorStep);

                if (!result.Succeeded || result.Data == null)
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định từ API";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi load data", msg);
                    return;
                }

                // Map từ APIClient DTO → Models để dùng computed FreqDiff
                _testRows = result.Data.Select((r, idx) => new AutoSandingTestRow
                {
                    RowIndex        = idx + 1,
                    Fre1            = r.Fre1,
                    BeltRotationRpm = r.BeltRotationRpm,
                    Fre2            = r.Fre2,
                    StiffnessY      = r.StiffnessY,
                }).ToList();

                _notificationService.Notify(NotificationSeverity.Success, "Load thành công",
                    $"Đã load {_testRows.Count} dòng từ DB (Part={partName}, WorkFre1={_workFre1}). Kiểm tra StiffnessY rồi nhấn Tính A,B,C,D.");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
            finally
            {
                _isLoadingCalc = false;
                StateHasChanged();
            }
        }

        private void OnCalculateABCD()
        {
            if (_testRows.Count < 2)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Thiếu dữ liệu", "Cần ít nhất 2 dòng để tính hồi quy.");
                return;
            }

            _resultAB = LinearRegression(_testRows.Select(r => r.StiffnessY).ToList(), _testRows.Select(r => r.Fre2).ToList());
            _resultCD = LinearRegression(_testRows.Select(r => r.FreqDiff).ToList(),   _testRows.Select(r => r.BeltRotationRpm).ToList());

            _chartABPoints = _testRows.Select(r => new ChartPoint { X = r.StiffnessY, Y = r.Fre2 }).ToList();
            _chartCDPoints = _testRows.Select(r => new ChartPoint { X = r.FreqDiff,   Y = r.BeltRotationRpm }).ToList();
            BuildTrendLine(_chartABPoints, _resultAB, out _trendABPoints);
            BuildTrendLine(_chartCDPoints, _resultCD, out _trendCDPoints);

            _abcdCalculated = true;
            StateHasChanged();
        }

        private async Task OnApplyAbcdToPart()
        {
            var part = _parts.FirstOrDefault(p => p.Id == _selectedPartId);
            if (part == null)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Chưa chọn Part", "Vui lòng chọn part trước khi áp dụng.");
                return;
            }

            try
            {
                _isSaving    = true;
                StateHasChanged();

                double a = Math.Round(_resultAB.Slope,     3);
                double b = Math.Round(_resultAB.Intercept,  3);
                // Z_Stiffness: nghịch đảo hàm Y = A*Z + B → Z = (Y - B) / A với Y = FreqTarget
                double freqTarget  = part.FreqTarget ?? 0;
                double zStiffness  = Math.Abs(a) > 1e-10
                    ? Math.Round((freqTarget - b) / a, 3)
                    : 0;

                part.A           = a;
                part.B           = b;
                part.C           = Math.Round(_resultCD.Slope,     3);
                part.D           = Math.Round(_resultCD.Intercept,  3);
                part.Z_Stiffness = zStiffness;
                part.Formula     = _formular;
                part.UpdateddAt  = DateTime.Now;

                var result = await _fT14Client.UpdateAsync(part);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Đã lưu DB",
                        $"Formula={_formular} | A={part.A}, B={part.B}, C={part.C}, D={part.D} | Z_Stiffness={part.Z_Stiffness} → part \"{part.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        // ── Toán học ─────────────────────────────────────────────────────────
        private static LinearRegressionResult LinearRegression(List<double> x, List<double> y)
        {
            int n = x.Count;
            double sumX  = x.Sum();
            double sumY  = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);
            double denom = n * sumX2 - sumX * sumX;
            double slope     = denom == 0 ? 0 : (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;
            double yMean = sumY / n;
            double ssTot = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssRes = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
            return new LinearRegressionResult { Slope = slope, Intercept = intercept, R2 = ssTot == 0 ? 1 : 1 - ssRes / ssTot };
        }

        private static void BuildTrendLine(List<ChartPoint> scatter, LinearRegressionResult reg, out List<ChartPoint> trend)
        {
            double xMin = scatter.Min(p => p.X), xMax = scatter.Max(p => p.X);
            trend = new List<ChartPoint>
            {
                new() { X = xMin, Y = reg.Slope * xMin + reg.Intercept },
                new() { X = xMax, Y = reg.Slope * xMax + reg.Intercept }
            };
        }

        public class ChartPoint { public double X { get; set; } public double Y { get; set; } }
    }
}
