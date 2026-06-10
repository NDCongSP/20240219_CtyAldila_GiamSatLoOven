using ClosedXML.Excel;
using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.UI.Pages
{
    public partial class AutoSandingReport
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;

        // ── Filter state ──────────────────────────────────────────────
        private int? _selectedMode = 1; // 1 = Production (mặc định)
        private DateTime? _fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        private DateTime? _toDate = DateTime.Today;

        private readonly List<ModeOption> _modeOptions = new()
        {
            new ModeOption { Label = "Tất cả",     Value = null },
            new ModeOption { Label = "Production", Value = 1    },
            new ModeOption { Label = "Test",        Value = 2   },
        };

        // ── Data ──────────────────────────────────────────────────────
        private List<FT16SandingLogData>? _data;
        private bool _isLoading = false;
        private bool _isExporting = false;

        // ── Handlers ─────────────────────────────────────────────────
        private async Task OnSearch()
        {
            _isLoading = true;
            try
            {
                var result = await _ft16ReportClient.GetReportAsync(_fromDate, _toDate, _selectedMode);
                if (result.Succeeded)
                {
                    _data = result.Data ?? new();
                    if (_data.Count == 0)
                        _notificationService.Notify(Radzen.NotificationSeverity.Info, "Thông báo", "Không có dữ liệu trong khoảng thời gian đã chọn.");
                }
                else
                {
                    _notificationService.Notify(Radzen.NotificationSeverity.Error, "Lỗi", result.Messages?.FirstOrDefault() ?? "Không thể tải dữ liệu.");
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task OnExportExcel()
        {
            if (_data == null || _data.Count == 0) return;
            _isExporting = true;
            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Sanding Report");

                // Headers
                var headers = new[]
                {
                    "Thời gian", "Part", "Work Order", "Shaft #", "Mode", "Formula",
                    "Motor Speed", "Spine A", "Spine B", "Target", "Spine LL", "Spine UL", "OK/NG Spine",
                    "OD 1 Reading", "OK/NG OD1",
                    "OD 2 Reading", "OK/NG OD2",
                    "OD 3 Reading", "OK/NG OD3"
                };
                for (int c = 0; c < headers.Length; c++)
                {
                    ws.Cell(1, c + 1).Value = headers[c];
                }

                var headerRow = ws.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

                // Data rows
                for (int i = 0; i < _data.Count; i++)
                {
                    var r = _data[i];
                    int row = i + 2;
                    ws.Cell(row, 1).Value = r.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    ws.Cell(row, 2).Value = r.Part ?? "";
                    ws.Cell(row, 3).Value = r.Work ?? "";
                    if (r.ShaftNum.HasValue) ws.Cell(row, 4).Value = r.ShaftNum.Value;
                    ws.Cell(row, 5).Value = r.SandingMode == 1 ? "Production" : r.SandingMode == 2 ? "Test" : "";
                    if (r.Formula.HasValue) ws.Cell(row, 6).Value = r.Formula.Value;
                    if (r.MotorSandingSpeed.HasValue) ws.Cell(row, 7).Value = r.MotorSandingSpeed.Value;
                    if (r.SpineA.HasValue) ws.Cell(row, 8).Value = r.SpineA.Value;
                    if (r.SpineB.HasValue) ws.Cell(row, 9).Value = r.SpineB.Value;
                    if (r.SpineTarget.HasValue) ws.Cell(row, 10).Value = r.SpineTarget.Value;
                    if (r.Spine_Low.HasValue) ws.Cell(row, 11).Value = r.Spine_Low.Value;
                    if (r.Spine_Hight.HasValue) ws.Cell(row, 12).Value = r.Spine_Hight.Value;
                    ws.Cell(row, 13).Value = r.OK_NG_SpineB == 1 ? "OK" : r.OK_NG_SpineB == 0 ? "NG" : "";
                    if (r.Diam_Reading_1.HasValue) ws.Cell(row, 14).Value = r.Diam_Reading_1.Value;
                    ws.Cell(row, 15).Value = r.OK_NG_OD_1 == 1 ? "OK" : r.OK_NG_OD_1 == 0 ? "NG" : "";
                    if (r.Diam_Reading_2.HasValue) ws.Cell(row, 16).Value = r.Diam_Reading_2.Value;
                    ws.Cell(row, 17).Value = r.OK_NG_OD_2 == 1 ? "OK" : r.OK_NG_OD_2 == 0 ? "NG" : "";
                    if (r.Diam_Reading_3.HasValue) ws.Cell(row, 18).Value = r.Diam_Reading_3.Value;
                    ws.Cell(row, 19).Value = r.OK_NG_OD_3 == 1 ? "OK" : r.OK_NG_OD_3 == 0 ? "NG" : "";
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                var fileName = $"SandingReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                await _js.InvokeVoidAsync("BlazorDownloadFile", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", base64);
            }
            finally
            {
                _isExporting = false;
            }
        }

        private class ModeOption
        {
            public string Label { get; set; } = "";
            public int? Value { get; set; }
        }
    }
}
