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
        public byte[] GenerateExcelFileAsync(List<FT09_RevoDatalog> data, string dateQuery, Pages.RevoReport.RevoReportMode mode)
        {
            using (var wb = new XLWorkbook())
            {
                wb.Properties.Author = "GiamSat System";
                wb.Properties.Title = "Báo cáo REVO";
                wb.Properties.Subject = "Dữ liệu báo cáo REVO";

                var ws = wb.Worksheets.Add("Báo cáo REVO");

                // Normalize (giống UI)
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

                // Build GLOBAL shaft sequential index (giống UI - không reset theo giờ)
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

                static string FormatDuration(TimeSpan ts)
                    => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                // Determine column count by mode
                int colCount = mode switch
                {
                    Pages.RevoReport.RevoReportMode.ByShaft => 9, // STT + ShaftLabel + Part + Work + Mandrel + StepCount + Start + End + TotalTime
                    Pages.RevoReport.RevoReportMode.ByHour  => 5,
                    _                                        => 11 // ByStep: STT + Shaft + RevoName + Part + Work + Rev + Mandrel + StepDisplay + Start + End + Duration
                };

                // ── Row 1: Title ──────────────────────────────────────────────────────────
                var titleCell = ws.Range(1, 1, 1, colCount).Merge();
                titleCell.Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetFontSize(16)
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#4caf50"));
                ws.Cell(1, 1).Value = "BÁO CÁO REVO";

                // ── Row 2: Date range ─────────────────────────────────────────────────────
                ws.Range(2, 1, 2, colCount).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell(2, 1).Value = $"Thời gian: {dateQuery}";

                // ── Row 3: Summary ────────────────────────────────────────────────────────
                var shaftCount = data.Select(x => x.ShaftNum).Where(x => x.HasValue).Distinct().Count();
                ws.Range(3, 1, 3, colCount).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetBold(true)
                    .Font.SetItalic(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#E3F2FD"));
                ws.Cell(3, 1).Value = $"Tổng số Shaft: {shaftCount}  |  Tổng số bản ghi: {data.Count}";

                var currentRow = 4;

                // ═════════════════════════════════════════════════════════════════════════
                // MODE: BY STEP
                // ═════════════════════════════════════════════════════════════════════════
                if (mode == Pages.RevoReport.RevoReportMode.ByStep)
                {
                    ws.Cell(currentRow, 1).Value  = "STT";
                    ws.Cell(currentRow, 2).Value  = "Shaft";
                    ws.Cell(currentRow, 3).Value  = "Tên REVO";
                    ws.Cell(currentRow, 4).Value  = "Part";
                    ws.Cell(currentRow, 5).Value  = "Work";
                    ws.Cell(currentRow, 6).Value  = "Rev";
                    ws.Cell(currentRow, 7).Value  = "Mandrel";
                    ws.Cell(currentRow, 8).Value  = "Tên Step (StepId)";
                    ws.Cell(currentRow, 9).Value  = "Bắt đầu";
                    ws.Cell(currentRow, 10).Value = "Kết thúc";
                    ws.Cell(currentRow, 11).Value = "Thời lượng";

                    StyleHeader(ws, currentRow, colCount);

                    var outRow = currentRow + 1;
                    var sttByShaft = new Dictionary<int, int>();

                    foreach (var x in normalized.OrderBy(x => x.Started))
                    {
                        var item      = x.Row;
                        var stepName  = string.IsNullOrWhiteSpace(item.StepName) ? "N/A" : item.StepName;
                        var stepIdTxt = item.StepId.HasValue ? item.StepId.Value.ToString() : "N/A";
                        var durTxt    = "N/A";

                        if (item.StartedAt.HasValue && item.EndedAt.HasValue)
                            durTxt = FormatDuration(item.EndedAt.Value - item.StartedAt.Value);
                        else if (item.StartedAt.HasValue)
                            durTxt = "Đang chạy...";

                        var shaftNo = ResolveGlobalShaftNo(item.ShaftNum);
                        var shaftLabel = shaftNo > 0 ? $"Shaft {shaftNo}" : "(Không xác định)";
                        if (!sttByShaft.ContainsKey(shaftNo)) sttByShaft[shaftNo] = 0;
                        sttByShaft[shaftNo]++;

                        ws.Cell(outRow, 1).Value  = sttByShaft[shaftNo];
                        ws.Cell(outRow, 2).Value  = shaftLabel;
                        ws.Cell(outRow, 3).Value  = item.RevoName ?? "N/A";
                        ws.Cell(outRow, 4).Value  = item.Part ?? "N/A";
                        ws.Cell(outRow, 5).Value  = item.Work ?? "N/A";
                        ws.Cell(outRow, 6).Value  = item.Rev ?? "N/A";
                        ws.Cell(outRow, 7).Value  = item.Mandrel ?? "N/A";
                        ws.Cell(outRow, 8).Value  = $"{stepName} ({stepIdTxt})";
                        ws.Cell(outRow, 9).Value  = item.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                        ws.Cell(outRow, 10).Value = item.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                        ws.Cell(outRow, 11).Value = durTxt;
                        outRow++;
                    }

                    StyleData(ws, currentRow, outRow - 1, colCount);
                }

                // ═════════════════════════════════════════════════════════════════════════
                // MODE: BY SHAFT (đồng bộ với UI: 1 dòng/shaft, thêm RevoName/Part/Work/Mandrel)
                // ═════════════════════════════════════════════════════════════════════════
                else if (mode == Pages.RevoReport.RevoReportMode.ByShaft)
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

                    // Cùng logic với UI RebuildView - group by ShaftNum; bao gồm shaft đã hoàn thành hoặc REVO "auto rolling" (chỉ TotalTime)
                    var shaftRows = normalized
                        .Where(x => x.Row.ShaftNum.HasValue)
                        .GroupBy(x => x.Row.ShaftNum!.Value)
                        .Where(g =>
                        {
                            var isAutoRolling = g.Any(x => (x.Row.RevoName ?? "").ToLowerInvariant().Contains("auto rolling"));
                            if (isAutoRolling) return true;
                            return g.All(x => x.Row.StartedAt.HasValue && x.Row.EndedAt.HasValue);
                        })
                        .Select(g =>
                        {
                            var firstRow = g.OrderBy(x => x.Started).First().Row;
                            var isAutoRolling = g.Any(x => (x.Row.RevoName ?? "").ToLowerInvariant().Contains("auto rolling"));

                            if (isAutoRolling)
                            {
                                var totalSeconds = g.Sum(x => x.Row.TotalTime ?? 0);
                                var totalTimeRolling = TimeSpan.FromSeconds(totalSeconds);
                                var orderKey = g.Min(x => x.Started);
                                return new
                                {
                                    ShaftGuid   = g.Key,
                                    FirstRow    = firstRow,
                                    StepCount   = g.Count(),
                                    StartedAt   = (DateTime?)null,
                                    EndedAt     = (DateTime?)null,
                                    TotalTime   = totalTimeRolling,
                                    OrderKey    = orderKey
                                };
                            }

                            var start = g.Min(x => x.Row.StartedAt)!.Value;
                            var end   = g.Max(x => x.Row.EndedAt)!.Value;
                            var totalTime = end - start;
                            return new
                            {
                                ShaftGuid   = g.Key,
                                FirstRow    = firstRow,
                                StepCount   = g.Count(),
                                StartedAt   = (DateTime?)start,
                                EndedAt     = (DateTime?)end,
                                TotalTime   = totalTime,
                                OrderKey    = start
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
                        outRow++;
                    }

                    StyleData(ws, currentRow, outRow - 1, colCount);
                }

                // ═════════════════════════════════════════════════════════════════════════
                // MODE: BY HOUR
                // ═════════════════════════════════════════════════════════════════════════
                else
                {
                    ws.Cell(currentRow, 1).Value = "Giờ";
                    ws.Cell(currentRow, 2).Value = "Tổng số Shaft";
                    ws.Cell(currentRow, 3).Value = "Bắt đầu";
                    ws.Cell(currentRow, 4).Value = "Kết thúc";
                    ws.Cell(currentRow, 5).Value = "Thời lượng";

                    StyleHeader(ws, currentRow, colCount);

                    var hourRows = normalized
                        .GroupBy(x => x.Hour)
                        .Select(g =>
                        {
                            var start      = g.Min(x => x.Row.StartedAt) ?? g.Min(x => x.Started);
                            var end        = g.Max(x => x.Row.EndedAt) ?? (DateTime?)null;
                            var endFb      = end ?? start;
                            var totalTime  = endFb - start;
                            var shaftCnt   = g.Select(x => x.Row.ShaftNum).Where(x => x.HasValue).Select(x => x!.Value).Distinct().Count();
                            return new
                            {
                                HourRange  = $"{g.Key:dd/MM/yyyy HH}:00-{g.Key.AddHours(1):HH}:00",
                                ShaftCount = shaftCnt,
                                StartedAt  = start,
                                EndedAt    = end,
                                TotalTime  = totalTime
                            };
                        })
                        .OrderBy(x => x.StartedAt)
                        .ToList();

                    var outRow = currentRow + 1;
                    foreach (var r in hourRows)
                    {
                        ws.Cell(outRow, 1).Value = r.HourRange;
                        ws.Cell(outRow, 2).Value = r.ShaftCount;
                        ws.Cell(outRow, 3).Value = r.StartedAt.ToString("dd/MM/yyyy HH:mm:ss");
                        ws.Cell(outRow, 4).Value = r.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                        ws.Cell(outRow, 5).Value = FormatDuration(r.TotalTime);
                        outRow++;
                    }

                    StyleData(ws, currentRow, outRow - 1, colCount);
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
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
