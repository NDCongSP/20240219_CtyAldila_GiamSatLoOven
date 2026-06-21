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

        private const int MaxRowsPerSheet = 1000000;

        private static IXLWorksheet CreateNewSheet(
            XLWorkbook wb,
            string baseName,
            int sheetIndex,
            string dateQuery,
            string shaftLine,
            int colCount)
        {
            string sheetName = sheetIndex == 1 ? baseName : " ()";
            if (sheetName.Length > 31)
            {
                sheetName = sheetName.Substring(0, 31);
            }
            var ws = wb.Worksheets.Add(sheetName);

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
            ws.Cell(2, 1).Value = "Thời gian: " + dateQuery;

            ws.Range(3, 1, 3, colCount).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Font.SetBold(true)
                .Font.SetItalic(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E3F2FD"));
            ws.Cell(3, 1).Value = shaftLine;

            return ws;
        }

        public byte[] GenerateExcelFileAsync(
            string dateQuery,
            string shaftLine,
            Pages.RevoReport.RevoReportMode mode,
            IReadOnlyList<Pages.RevoReport.RevoStepRow>? stepRows,
            IReadOnlyList<Pages.RevoReport.RevoShaftRow>? shaftRows,
            IReadOnlyList<Pages.RevoReport.RevoHourRow>? hourRows,
            List<Pages.RevoReport.RevoDropdownModel>? revoList = null)
        {
            using var wb = new XLWorkbook();
            wb.Properties.Author = "GiamSat System";
            wb.Properties.Title = "Báo cáo REVO";
            wb.Properties.Subject = "Dữ liệu báo cáo REVO";

            int colCount = mode switch
            {
                Pages.RevoReport.RevoReportMode.ByShaft => 9,
                Pages.RevoReport.RevoReportMode.ByHour => 1 + (revoList?.Count ?? 0) * 2,
                _ => 12
            };

            if (mode == Pages.RevoReport.RevoReportMode.ByStep && stepRows != null)
            {
                WriteStepFromGrid(wb, "Báo cáo REVO", dateQuery, shaftLine, colCount, stepRows);
            }
            else if (mode == Pages.RevoReport.RevoReportMode.ByShaft && shaftRows != null)
            {
                WriteShaftFromGrid(wb, "Báo cáo REVO", dateQuery, shaftLine, colCount, shaftRows);
            }
            else if (mode == Pages.RevoReport.RevoReportMode.ByHour && hourRows != null && revoList != null)
            {
                var ws = CreateNewSheet(wb, "Báo cáo REVO", 1, dateQuery, shaftLine, colCount);
                WriteHourFromGrid(ws, 4, hourRows, revoList);
            }

            foreach (var worksheet in wb.Worksheets)
            {
                worksheet.Columns().AdjustToContents();
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static void WriteStepFromGrid(XLWorkbook wb, string baseName, string dateQuery, string shaftLine, int colCount, IReadOnlyList<Pages.RevoReport.RevoStepRow> rows)
        {
            int sheetIndex = 1;
            var ws = CreateNewSheet(wb, baseName, sheetIndex, dateQuery, shaftLine, colCount);

            int currentRow = 4;
            ws.Cell(currentRow, 1).Value = "STT";
            ws.Cell(currentRow, 2).Value = "Shaft";
            ws.Cell(currentRow, 3).Value = "Tên REVO";
            ws.Cell(currentRow, 4).Value = "Part";
            ws.Cell(currentRow, 5).Value = "Work";
            ws.Cell(currentRow, 6).Value = "Rev";
            ws.Cell(currentRow, 7).Value = "Mandrel";
            ws.Cell(currentRow, 8).Value = "Step";
            ws.Cell(currentRow, 9).Value = "Tên Step";
            ws.Cell(currentRow, 10).Value = "Bắt đầu";
            ws.Cell(currentRow, 11).Value = "Kết thúc";
            ws.Cell(currentRow, 12).Value = "Thời lượng";
            StyleHeader(ws, currentRow, colCount);

            var outRow = currentRow + 1;
            foreach (var r in rows)
            {
                if (outRow > MaxRowsPerSheet)
                {
                    StyleData(ws, currentRow, outRow - 1, colCount);
                    sheetIndex++;
                    ws = CreateNewSheet(wb, baseName, sheetIndex, dateQuery, shaftLine, colCount);
                    currentRow = 4;
                    ws.Cell(currentRow, 1).Value = "STT";
                    ws.Cell(currentRow, 2).Value = "Shaft";
                    ws.Cell(currentRow, 3).Value = "Tên REVO";
                    ws.Cell(currentRow, 4).Value = "Part";
                    ws.Cell(currentRow, 5).Value = "Work";
                    ws.Cell(currentRow, 6).Value = "Rev";
                    ws.Cell(currentRow, 7).Value = "Mandrel";
                    ws.Cell(currentRow, 8).Value = "Step";
                    ws.Cell(currentRow, 9).Value = "Tên Step";
                    ws.Cell(currentRow, 10).Value = "Bắt đầu";
                    ws.Cell(currentRow, 11).Value = "Kết thúc";
                    ws.Cell(currentRow, 12).Value = "Thời lượng";
                    StyleHeader(ws, currentRow, colCount);
                    outRow = currentRow + 1;
                }

                ws.Cell(outRow, 1).Value = r.Stt;
                ws.Cell(outRow, 2).Value = r.ShaftKey;
                ws.Cell(outRow, 3).Value = r.RevoName ?? "N/A";
                ws.Cell(outRow, 4).Value = r.Part ?? "N/A";
                ws.Cell(outRow, 5).Value = r.Work ?? "N/A";
                ws.Cell(outRow, 6).Value = r.Rev ?? "N/A";
                ws.Cell(outRow, 7).Value = r.Mandrel ?? "N/A";
                ws.Cell(outRow, 8).Value = r.StepId?.ToString() ?? "N/A";
                ws.Cell(outRow, 9).Value = r.StepDisplay;
                ws.Cell(outRow, 10).Value = r.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 11).Value = r.EndedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 12).Value = r.DurationText;
                if (r.HighlightIncomplete)
                    ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                outRow++;
            }
            StyleData(ws, currentRow, outRow - 1, colCount);
        }

        private static void WriteShaftFromGrid(XLWorkbook wb, string baseName, string dateQuery, string shaftLine, int colCount, IReadOnlyList<Pages.RevoReport.RevoShaftRow> rows)
        {
            int sheetIndex = 1;
            var ws = CreateNewSheet(wb, baseName, sheetIndex, dateQuery, shaftLine, colCount);

            int currentRow = 4;
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
                if (outRow > MaxRowsPerSheet)
                {
                    StyleData(ws, currentRow, outRow - 1, colCount);
                    sheetIndex++;
                    ws = CreateNewSheet(wb, baseName, sheetIndex, dateQuery, shaftLine, colCount);
                    currentRow = 4;
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
                    outRow = currentRow + 1;
                }

                ws.Cell(outRow, 1).Value = r.Stt;
                ws.Cell(outRow, 2).Value = r.ShaftLabel;
                ws.Cell(outRow, 3).Value = r.Part ?? "N/A";
                ws.Cell(outRow, 4).Value = r.Work ?? "N/A";
                ws.Cell(outRow, 5).Value = r.Mandrel ?? "N/A";
                ws.Cell(outRow, 6).Value = r.StepCount;
                ws.Cell(outRow, 7).Value = r.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 8).Value = r.EndedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                ws.Cell(outRow, 9).Value = r.TotalTimeText;
                if (r.HighlightIncomplete)
                    ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                outRow++;
            }
            StyleData(ws, currentRow, outRow - 1, colCount);
        }

        private static void WriteHourFromGrid(
            IXLWorksheet ws,
            int currentRow,
            IReadOnlyList<Pages.RevoReport.RevoHourRow> rows,
            List<Pages.RevoReport.RevoDropdownModel> revoList)
        {
            int colCount = 1 + revoList.Count * 2;

            // Merge "Giờ" vertically across currentRow and currentRow + 1
            ws.Range(currentRow, 1, currentRow + 1, 1).Merge().Value = "Giờ";
            ws.Range(currentRow, 1, currentRow + 1, 1).Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            int currentCol = 2;
            foreach (var revo in revoList)
            {
                ws.Range(currentRow, currentCol, currentRow, currentCol + 1).Merge().Value = revo.Name;
                ws.Cell(currentRow + 1, currentCol).Value = "Tổng số";
                ws.Cell(currentRow + 1, currentCol + 1).Value = "Hoàn thành";
                currentCol += 2;
            }

            StyleHeader(ws, currentRow, colCount, XLColor.FromHtml("#FFF2CC"));
            StyleHeader(ws, currentRow + 1, colCount, XLColor.FromHtml("#FFF2CC"));

            var outRow = currentRow + 2;
            foreach (var r in rows)
            {
                ws.Cell(outRow, 1).Value = r.HourRange;

                int cCol = 2;
                foreach (var revo in revoList)
                {
                    var stats = r.GetMachineStats(revo.Id);
                    ws.Cell(outRow, cCol).Value = stats.TotalShafts;
                    ws.Cell(outRow, cCol + 1).Value = stats.FinishedShafts;
                    cCol += 2;
                }

                if (r.HighlightIncomplete)
                    ws.Range(outRow, 1, outRow, colCount).Style.Fill.SetBackgroundColor(WarningFill);
                outRow++;
            }
            StyleData(ws, currentRow, outRow - 1, colCount);
        }

        private static void StyleHeader(IXLWorksheet ws, int row, int colCount, XLColor? bgColor = null)
        {
            var r = ws.Range(row, 1, row, colCount);
            r.Style.Font.SetBold(true)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Fill.SetBackgroundColor(bgColor ?? XLColor.FromHtml("#EFEFEF"));
            r.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
             .Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        private static void StyleData(IXLWorksheet ws, int headerRow, int lastRow, int colCount)
        {
            if (lastRow > headerRow)
            {
                var r = ws.Range(headerRow + 1, 1, lastRow, colCount);
                r.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                r.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Border.SetInsideBorder(XLBorderStyleValues.Thin);
            }
        }
    }
}
