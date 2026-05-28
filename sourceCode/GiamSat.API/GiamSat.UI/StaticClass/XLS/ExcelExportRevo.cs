using ClosedXML.Excel;
using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GiamSat.UI
{
    public class ExcelExportRevo
    {
        // Màu nền warning (cam nhạt, gần Material orange 50)
        private static readonly XLColor WarningFill = XLColor.FromHtml("#FFF3E0");

        /// <summary>Xuất Excel theo mode; có thể dùng dữ liệu lưới (TVF) hoặc tổng hợp từ FT09.</summary>
        public byte[] GenerateExcelFileAsync(
            List<FT09_RevoDatalog> data,
            string dateQuery,
            Pages.RevoReport.RevoReportMode mode,
            Pages.RevoReport.RevoShaftScopeKind shaftScope,
            bool useGridWhenAvailable,
            IReadOnlyList<Pages.RevoReport.RevoStepRow>? stepRows,
            IReadOnlyList<Pages.RevoReport.RevoShaftRow>? shaftRows,
            IReadOnlyList<Pages.RevoReport.RevoHourRow>? hourRows)
        {
            using var wb = new XLWorkbook();
            wb.Properties.Author = "GiamSat System";
            wb.Properties.Title = "Báo cáo REVO";
            wb.Properties.Subject = "Dữ liệu báo cáo REVO";

            var ws = wb.Worksheets.Add("Báo cáo REVO");

            var scopeLabel = shaftScope == Pages.RevoReport.RevoShaftScopeKind.Finished
                ? "Chỉ shaft hoàn thành"
                : "Tất cả shaft";

            int colCount = mode switch
            {
                Pages.RevoReport.RevoReportMode.ByShaft => 9,
                Pages.RevoReport.RevoReportMode.ByHour => 7,
                _ => 11
            };

            ws.Range(1, 1, 1, colCount).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Font.SetFontSize(16)
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#4caf50"));
            ws.Cell(1, 1).Value = "BÁO CÁO REVO";

            ws.Range(2, 1, 2, colCount).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Cell(2, 1).Value = $"Thời gian: {dateQuery}";

            var shaftTotal = data.Where(x => x.ShaftNum.HasValue).Select(x => x.ShaftNum!.Value).Distinct().Count();
            var shaftFinished = CountFinishedShafts(data);
            var shaftLine = shaftScope == Pages.RevoReport.RevoShaftScopeKind.Finished
                ? $"Phạm vi: {scopeLabel}  |  Shaft (theo phạm vi): {shaftFinished}  |  Tổng số bản ghi: {data.Count}"
                : $"Phạm vi: {scopeLabel}  |  Tổng số Shaft: {shaftTotal}  |  Tổng số bản ghi: {data.Count}";

            ws.Range(3, 1, 3, colCount).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Font.SetBold(true)
                .Font.SetItalic(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E3F2FD"));
            ws.Cell(3, 1).Value = shaftLine;

            var currentRow = 4;

            if (useGridWhenAvailable && mode == Pages.RevoReport.RevoReportMode.ByStep && stepRows is { Count: > 0 })
                currentRow = WriteStepFromGrid(ws, currentRow, colCount, stepRows);
            else if (useGridWhenAvailable && mode == Pages.RevoReport.RevoReportMode.ByShaft && shaftRows is { Count: > 0 })
                currentRow = WriteShaftFromGrid(ws, currentRow, colCount, shaftRows);
            else if (useGridWhenAvailable && mode == Pages.RevoReport.RevoReportMode.ByHour && hourRows is { Count: > 0 })
                currentRow = WriteHourFromGrid(ws, currentRow, hourRows);
            else
                currentRow = WriteFromRawFt09(ws, data, mode, shaftScope, currentRow, colCount);

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static int WriteStepFromGrid(IXLWorksheet ws, int currentRow, int colCount, IReadOnlyList<Pages.RevoReport.RevoStepRow> rows)
        {
            ws.Cell(currentRow, 1).Value = "STT";
            ws.Cell(currentRow, 2).Value = "Shaft";
            ws.Cell(currentRow, 3).Value = "Tên REVO";
            ws.Cell(currentRow, 4).Value = "Part";
            ws.Cell(currentRow, 5).Value = "Work";
            ws.Cell(currentRow, 6).Value = "Rev";
            ws.Cell(currentRow, 7).Value = "Mandrel";
            ws.Cell(currentRow, 8).Value = "Tên Step (StepId)";
            ws.Cell(currentRow, 9).Value = "Bắt đầu";
            ws.Cell(currentRow, 10).Value = "Kết thúc";
            ws.Cell(currentRow, 11).Value = "Thời lượng";
            StyleHeader(ws, currentRow, colCount);
            var outRow = currentRow + 1;
            foreach (var r in rows)
            {
                ws.Cell(outRow, 1).Value = r.Stt;
                ws.Cell(outRow, 2).Value = r.ShaftKey;
                ws.Cell(outRow, 3).Value = r.RevoName ?? "N/A";
                ws.Cell(outRow, 4).Value = r.Part ?? "N/A";
                ws.Cell(outRow, 5).Value = r.Work ?? "N/A";
                ws.Cell(outRow, 6).Value = r.Rev ?? "N/A";
                ws.Cell(outRow, 7).Value = r.Mandrel ?? "N/A";
                ws.Cell(outRow, 8).Value = r.StepDisplay;
                ws.Cell(outRow, 9).Value = r.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 10).Value = r.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 11).Value = r.DurationText;
                if (r.HighlightIncomplete)
                    ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                outRow++;
            }
            StyleData(ws, currentRow, outRow - 1, colCount);
            return outRow;
        }

        private static int WriteShaftFromGrid(IXLWorksheet ws, int currentRow, int colCount, IReadOnlyList<Pages.RevoReport.RevoShaftRow> rows)
        {
            ws.Cell(currentRow, 1).Value = "STT";
            ws.Cell(currentRow, 2).Value = "Tên Shaft";
            ws.Cell(currentRow, 3).Value = "Part";
            ws.Cell(currentRow, 4).Value = "Work";
            ws.Cell(currentRow, 5).Value = "Mandrel";
            ws.Cell(currentRow, 6).Value = "Số step";
            ws.Cell(currentRow, 7).Value = "Bắt đầu";
            ws.Cell(currentRow, 8).Value = "Kết thúc";
            ws.Cell(currentRow, 9).Value = "Thời lượng";
            StyleHeader(ws, currentRow, colCount);
            var outRow = currentRow + 1;
            foreach (var r in rows)
            {
                ws.Cell(outRow, 1).Value = r.Stt;
                ws.Cell(outRow, 2).Value = r.ShaftLabel;
                ws.Cell(outRow, 3).Value = r.Part ?? "N/A";
                ws.Cell(outRow, 4).Value = r.Work ?? "N/A";
                ws.Cell(outRow, 5).Value = r.Mandrel ?? "N/A";
                ws.Cell(outRow, 6).Value = r.StepCount;
                ws.Cell(outRow, 7).Value = r.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 8).Value = r.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 9).Value = r.TotalTimeText;
                if (r.HighlightIncomplete)
                    ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                outRow++;
            }
            StyleData(ws, currentRow, outRow - 1, colCount);
            return outRow;
        }

        private static int WriteHourFromGrid(IXLWorksheet ws, int currentRow, IReadOnlyList<Pages.RevoReport.RevoHourRow> rows)
        {
            const int colCount = 7;
            ws.Cell(currentRow, 1).Value = "Giờ";
            ws.Cell(currentRow, 2).Value = "Tổng số Shaft";
            ws.Cell(currentRow, 3).Value = "Hoàn thành (trong giờ)";
            ws.Cell(currentRow, 4).Value = "Chưa xong (giờ)";
            ws.Cell(currentRow, 5).Value = "Bắt đầu";
            ws.Cell(currentRow, 6).Value = "Kết thúc";
            ws.Cell(currentRow, 7).Value = "Thời lượng";
            StyleHeader(ws, currentRow, colCount);
            var outRow = currentRow + 1;
            foreach (var r in rows)
            {
                ws.Cell(outRow, 1).Value = r.HourRange;
                ws.Cell(outRow, 2).Value = r.ShaftCount;
                ws.Cell(outRow, 3).Value = r.ShaftCountFinishedInHour;
                ws.Cell(outRow, 4).Value = r.IncompleteShaftCountInHour;
                ws.Cell(outRow, 5).Value = r.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 6).Value = r.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 7).Value = r.TotalHoursText;
                if (r.HighlightIncomplete)
                    ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                outRow++;
            }
            StyleData(ws, currentRow, outRow - 1, colCount);
            return outRow;
        }

        private static int WriteFromRawFt09(
            IXLWorksheet ws,
            List<FT09_RevoDatalog> data,
            Pages.RevoReport.RevoReportMode mode,
            Pages.RevoReport.RevoShaftScopeKind shaftScope,
            int currentRow,
            int colCount)
        {
            var normalized = data
                .Where(x => x != null)
                .Select(x =>
                {
                    var t = x.StartedAt ?? x.CreatedAt ?? DateTime.MinValue;
                    var hour = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
                    return new { Row = x, Started = t, Hour = hour };
                })
                .Where(x => x.Started != DateTime.MinValue)
                .ToList();

            var finishedShafts = BuildFinishedShaftSet(data);
            if (shaftScope == Pages.RevoReport.RevoShaftScopeKind.Finished)
            {
                normalized = normalized
                    .Where(x => !x.Row.ShaftNum.HasValue || finishedShafts.Contains(x.Row.ShaftNum.Value))
                    .ToList();
            }

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

            if (mode == Pages.RevoReport.RevoReportMode.ByStep)
            {
                ws.Cell(currentRow, 1).Value = "STT";
                ws.Cell(currentRow, 2).Value = "Shaft";
                ws.Cell(currentRow, 3).Value = "Tên REVO";
                ws.Cell(currentRow, 4).Value = "Part";
                ws.Cell(currentRow, 5).Value = "Work";
                ws.Cell(currentRow, 6).Value = "Rev";
                ws.Cell(currentRow, 7).Value = "Mandrel";
                ws.Cell(currentRow, 8).Value = "Tên Step (StepId)";
                ws.Cell(currentRow, 9).Value = "Bắt đầu";
                ws.Cell(currentRow, 10).Value = "Kết thúc";
                ws.Cell(currentRow, 11).Value = "Thời lượng";
                StyleHeader(ws, currentRow, colCount);

                var outRow = currentRow + 1;
                var sttByShaft = new Dictionary<int, int>();
                foreach (var x in normalized.OrderBy(x => x.Started))
                {
                    var item = x.Row;
                    var stepName = string.IsNullOrWhiteSpace(item.StepName) ? "N/A" : item.StepName;
                    var stepIdTxt = item.StepId.HasValue ? item.StepId.Value.ToString() : "N/A";
                    var isAutoRolling = IsAutoRolling(item);
                    var durTxt = "N/A";
                    if (isAutoRolling)
                    {
                        var total = item.TotalTime ?? 0;
                        durTxt = total > 0 ? FormatDuration(TimeSpan.FromSeconds(total)) : "N/A";
                    }
                    else if (item.StartedAt.HasValue && item.EndedAt.HasValue)
                        durTxt = FormatDuration(item.EndedAt.Value - item.StartedAt.Value);
                    else if (item.StartedAt.HasValue)
                        durTxt = "Đang chạy...";

                    var shaftNo = ResolveGlobalShaftNo(item.ShaftNum);
                    var shaftLabel = shaftNo > 0 ? $"Shaft {shaftNo}" : "(Không xác định)";
                    if (!sttByShaft.ContainsKey(shaftNo)) sttByShaft[shaftNo] = 0;
                    sttByShaft[shaftNo]++;

                    ws.Cell(outRow, 1).Value = sttByShaft[shaftNo];
                    ws.Cell(outRow, 2).Value = shaftLabel;
                    ws.Cell(outRow, 3).Value = item.RevoName ?? "N/A";
                    ws.Cell(outRow, 4).Value = item.Part ?? "N/A";
                    ws.Cell(outRow, 5).Value = item.Work ?? "N/A";
                    ws.Cell(outRow, 6).Value = item.Rev ?? "N/A";
                    ws.Cell(outRow, 7).Value = item.Mandrel ?? "N/A";
                    ws.Cell(outRow, 8).Value = $"{stepName} ({stepIdTxt})";
                    ws.Cell(outRow, 9).Value = isAutoRolling ? "N/A" : item.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                    ws.Cell(outRow, 10).Value = isAutoRolling ? "N/A" : item.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                    ws.Cell(outRow, 11).Value = durTxt;

                    var warn = shaftScope == Pages.RevoReport.RevoShaftScopeKind.Total
                        && item.ShaftNum.HasValue
                        && !finishedShafts.Contains(item.ShaftNum.Value);
                    if (warn)
                        ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                    outRow++;
                }
                StyleData(ws, currentRow, outRow - 1, colCount);
                return outRow;
            }

            if (mode == Pages.RevoReport.RevoReportMode.ByShaft)
            {
                ws.Cell(currentRow, 1).Value = "STT";
                ws.Cell(currentRow, 2).Value = "Tên Shaft";
                ws.Cell(currentRow, 3).Value = "Part";
                ws.Cell(currentRow, 4).Value = "Work";
                ws.Cell(currentRow, 5).Value = "Mandrel";
                ws.Cell(currentRow, 6).Value = "Số step";
                ws.Cell(currentRow, 7).Value = "Bắt đầu";
                ws.Cell(currentRow, 8).Value = "Kết thúc";
                ws.Cell(currentRow, 9).Value = "Thời lượng";
                StyleHeader(ws, currentRow, colCount);

                var shaftRows = normalized
                    .Where(x => x.Row.ShaftNum.HasValue)
                    .GroupBy(x => x.Row.ShaftNum!.Value)
                    .Select(g =>
                    {
                        var firstRow = g.OrderBy(x => x.Started).First().Row;
                        var isAutoRolling = g.Any(x => IsAutoRolling(x.Row));
                        var totalSeconds = g.Sum(x => x.Row.TotalTime ?? 0);
                        var totalTime = totalSeconds > 0 ? TimeSpan.FromSeconds(totalSeconds) : TimeSpan.Zero;
                        DateTime orderKey;
                        if (isAutoRolling)
                        {
                            orderKey = g.Min(x => x.Started);
                            return new
                            {
                                ShaftGuid = g.Key,
                                FirstRow = firstRow,
                                StepCount = g.Count(),
                                StartedAt = (DateTime?)null,
                                EndedAt = (DateTime?)null,
                                TotalTime = totalTime,
                                OrderKey = orderKey
                            };
                        }
                        var start = g.Min(x => x.Row.StartedAt);
                        var end = g.Max(x => x.Row.EndedAt);
                        orderKey = start ?? g.Min(x => x.Started);
                        return new
                        {
                            ShaftGuid = g.Key,
                            FirstRow = firstRow,
                            StepCount = g.Count(),
                            StartedAt = start,
                            EndedAt = end,
                            TotalTime = totalTime,
                            OrderKey = orderKey
                        };
                    })
                    .OrderBy(x => x.OrderKey)
                    .ToList();

                var outRow = currentRow + 1;
                var stt = 0;
                foreach (var r in shaftRows)
                {
                    stt++;
                    ws.Cell(outRow, 1).Value = stt;
                    ws.Cell(outRow, 2).Value = $"Shaft {stt}";
                    ws.Cell(outRow, 3).Value = r.FirstRow.Part ?? "N/A";
                    ws.Cell(outRow, 4).Value = r.FirstRow.Work ?? "N/A";
                    ws.Cell(outRow, 5).Value = r.FirstRow.Mandrel ?? "N/A";
                    ws.Cell(outRow, 6).Value = r.StepCount;
                    ws.Cell(outRow, 7).Value = r.StartedAt.HasValue ? r.StartedAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A";
                    ws.Cell(outRow, 8).Value = r.EndedAt.HasValue ? r.EndedAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A";
                    ws.Cell(outRow, 9).Value = FormatDuration(r.TotalTime);
                    var warn = shaftScope == Pages.RevoReport.RevoShaftScopeKind.Total && !finishedShafts.Contains(r.ShaftGuid);
                    if (warn)
                        ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                    outRow++;
                }
                StyleData(ws, currentRow, outRow - 1, colCount);
                return outRow;
            }

            // By hour — 7 cột (thêm hoàn thành / chưa xong trong giờ)
            const int hourCols = 7;
            ws.Cell(currentRow, 1).Value = "Giờ";
            ws.Cell(currentRow, 2).Value = "Tổng số Shaft";
            ws.Cell(currentRow, 3).Value = "Hoàn thành (trong giờ)";
            ws.Cell(currentRow, 4).Value = "Chưa xong (giờ)";
            ws.Cell(currentRow, 5).Value = "Bắt đầu";
            ws.Cell(currentRow, 6).Value = "Kết thúc";
            ws.Cell(currentRow, 7).Value = "Thời lượng";
            StyleHeader(ws, currentRow, hourCols);

            var firstHourByShaft = normalized
                .Where(x => x.Row.ShaftNum.HasValue)
                .GroupBy(x => x.Row.ShaftNum!.Value)
                .ToDictionary(h => h.Key, h => h.Min(x => x.Hour));

            var hourRows = normalized
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
                    var totalTime = totalSeconds > 0 ? TimeSpan.FromSeconds(totalSeconds) : TimeSpan.Zero;
                    var shaftCnt = g.Select(x => x.Row.ShaftNum).Where(x => x.HasValue).Select(x => x!.Value).Distinct().Count();
                    var distinctInHour = g.Where(x => x.Row.ShaftNum.HasValue).Select(x => x.Row.ShaftNum!.Value).Distinct().ToList();
                    var finishedInHour = distinctInHour.Count(sn =>
                        finishedShafts.Contains(sn)
                        && firstHourByShaft.TryGetValue(sn, out var fh)
                        && fh == g.Key);
                    var incompleteInHour = distinctInHour.Count(sn => !finishedShafts.Contains(sn));
                    return new
                    {
                        HourRange = $"{g.Key:dd/MM/yyyy HH}:00-{g.Key.AddHours(1):HH}:00",
                        ShaftCount = shaftCnt,
                        FinishedInHour = finishedInHour,
                        Incomplete = incompleteInHour,
                        StartedAt = start,
                        EndedAt = end,
                        TotalTime = totalTime,
                        Warn = shaftScope == Pages.RevoReport.RevoShaftScopeKind.Total && incompleteInHour > 0
                    };
                })
                .OrderBy(x => x.StartedAt)
                .ToList();

            var hr = currentRow + 1;
            foreach (var r in hourRows)
            {
                ws.Cell(hr, 1).Value = r.HourRange;
                ws.Cell(hr, 2).Value = r.ShaftCount;
                ws.Cell(hr, 3).Value = r.FinishedInHour;
                ws.Cell(hr, 4).Value = r.Incomplete;
                ws.Cell(hr, 5).Value = r.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(hr, 6).Value = r.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                ws.Cell(hr, 7).Value = FormatDuration(r.TotalTime);
                if (r.Warn)
                    ws.Range(hr, 1, hr, hourCols).Style.Fill.SetBackgroundColor(WarningFill);
                hr++;
            }
            StyleData(ws, currentRow, hr - 1, hourCols);
            return hr;
        }

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

        private static string FormatDuration(TimeSpan ts)
            => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

        private static bool IsAutoRolling(FT09_RevoDatalog row)
        {
            var revo = row.RevoName ?? string.Empty;
            var work = row.Work ?? string.Empty;
            return revo.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || work.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || revo.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase)
                || work.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase);
        }

        private static void StyleHeader(IXLWorksheet ws, int row, int colCount)
        {
            ws.Range(row, 1, row, colCount).SetAutoFilter(true);
            ws.Range(row, 1, row, colCount).Style
                .Fill.SetBackgroundColor(XLColor.LightCyan)
                .Font.SetBold(true)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        }

        private static void StyleData(IXLWorksheet ws, int headerRow, int lastRow, int colCount)
        {
            if (lastRow < headerRow) return;
            ws.Range(headerRow, 1, lastRow, colCount).Style
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
    }
}
