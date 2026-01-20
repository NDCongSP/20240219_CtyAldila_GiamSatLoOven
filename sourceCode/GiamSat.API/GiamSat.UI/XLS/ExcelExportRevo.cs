using ClosedXML.Excel;
using GiamSat.Models;
using System.Drawing;

namespace GiamSat.UI
{
    public class ExcelExportRevo
    {
        public byte[] GenerateExcelFileAsync(List<FT09_RevoDatalog> data, string dateQuery)
        {
            using (var wb = new XLWorkbook())
            {
                wb.Properties.Author = "GiamSat System";
                wb.Properties.Title = "Báo cáo REVO";
                wb.Properties.Subject = "Dữ liệu báo cáo REVO";

                var ws = wb.Worksheets.Add("Báo cáo REVO");

                // Header
                ws.Range(1, 1, 1, 10).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetFontSize(16).Font.SetBold(true);
                ws.Cell(1, 1).Value = "BÁO CÁO REVO";
                ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4caf50");

                ws.Range(2, 1, 2, 10).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                   .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell(2, 1).Value = $"Thời gian: {dateQuery}";

                // Column headers (Tiếng Việt)
                ws.Cell(3, 1).Value = "Tên REVO";
                ws.Cell(3, 2).Value = "Part";
                ws.Cell(3, 3).Value = "Rev";
                ws.Cell(3, 4).Value = "Màu";
                ws.Cell(3, 5).Value = "Mandrel";
                ws.Cell(3, 6).Value = "Tên Step";
                ws.Cell(3, 7).Value = "Thời gian bắt đầu";
                ws.Cell(3, 8).Value = "Thời gian kết thúc";
                ws.Cell(3, 9).Value = "Thời lượng";
                ws.Cell(3, 10).Value = "Work";

                ws.Range(3, 1, 3, 10).SetAutoFilter(true);
                ws.Range(3, 1, 3, 10).Style.Fill.BackgroundColor = XLColor.LightCyan;
                ws.Range(3, 1, 3, 10).Style.Font.SetBold(true);

                ws.Range($"A3:J{data.Count + 3}").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                var row = 0;
                foreach (var item in data)
                {
                    ws.Cell(row + 4, 1).Value = item.RevoName ?? "N/A";
                    ws.Cell(row + 4, 2).Value = item.Part ?? "N/A";
                    ws.Cell(row + 4, 3).Value = item.Rev ?? "N/A";
                    ws.Cell(row + 4, 4).Value = item.ColorCode ?? "N/A";
                    ws.Cell(row + 4, 5).Value = item.Mandrel ?? "N/A";
                    ws.Cell(row + 4, 6).Value = item.StepName ?? "N/A";
                    ws.Cell(row + 4, 7).Value = item.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                    ws.Cell(row + 4, 8).Value = item.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                    
                    if (item.StartedAt.HasValue && item.EndedAt.HasValue)
                    {
                        var duration = item.EndedAt.Value - item.StartedAt.Value;
                        ws.Cell(row + 4, 9).Value = duration.ToString(@"hh\:mm\:ss");
                    }
                    else if (item.StartedAt.HasValue)
                    {
                        ws.Cell(row + 4, 9).Value = "Đang chạy...";
                    }
                    else
                    {
                        ws.Cell(row + 4, 9).Value = "N/A";
                    }
                    
                    ws.Cell(row + 4, 10).Value = item.Work ?? "N/A";

                    row += 1;
                }

                ws.Columns().AdjustToContents();

                var bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    bytes = ms.ToArray();
                }

                return bytes;
            }
        }
    }
}
