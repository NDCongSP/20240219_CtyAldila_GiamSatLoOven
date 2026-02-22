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
                ws.Range(1, 1, 1, 12).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetFontSize(16).Font.SetBold(true);
                ws.Cell(1, 1).Value = "BÁO CÁO REVO";
                ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4caf50");

                ws.Range(2, 1, 2, 12).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                   .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                ws.Cell(2, 1).Value = $"Thời gian: {dateQuery}";

                // Summary row (row 3) - đưa lên trên đầu báo cáo
                var shaftCount = data.Select(x => x.ShaftNum).Where(x => x.HasValue).Distinct().Count();
                ws.Range(3, 1, 3, 12).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetBold(true)
                    .Font.SetItalic(true);
                ws.Cell(3, 1).Value = $"Tổng số Shaft: {shaftCount}  |  Tổng số bản ghi: {data.Count}";
                ws.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");

                // Column headers (Tiếng Việt) - row 4
                ws.Cell(4, 1).Value = "Tên REVO";
                ws.Cell(4, 2).Value = "ShaftNum";
                ws.Cell(4, 3).Value = "StepId";
                ws.Cell(4, 4).Value = "Part";
                ws.Cell(4, 5).Value = "Rev";
                ws.Cell(4, 6).Value = "Màu";
                ws.Cell(4, 7).Value = "Mandrel";
                ws.Cell(4, 8).Value = "Tên Step";
                ws.Cell(4, 9).Value = "Thời gian bắt đầu";
                ws.Cell(4, 10).Value = "Thời gian kết thúc";
                ws.Cell(4, 11).Value = "Thời lượng";
                ws.Cell(4, 12).Value = "Work";

                ws.Range(4, 1, 4, 12).SetAutoFilter(true);
                ws.Range(4, 1, 4, 12).Style.Fill.BackgroundColor = XLColor.LightCyan;
                ws.Range(4, 1, 4, 12).Style.Font.SetBold(true);

                ws.Range($"A4:L{data.Count + 4}").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                // Data rows start at row 5
                var row = 0;
                foreach (var item in data)
                {
                    ws.Cell(row + 5, 1).Value = item.RevoName ?? "N/A";
                    ws.Cell(row + 5, 2).Value = item.ShaftNum?.ToString() ?? "N/A";
                    ws.Cell(row + 5, 3).Value = item.StepId?.ToString() ?? "N/A";
                    ws.Cell(row + 5, 4).Value = item.Part ?? "N/A";
                    ws.Cell(row + 5, 5).Value = item.Rev ?? "N/A";
                    ws.Cell(row + 5, 6).Value = item.ColorCode ?? "N/A";
                    ws.Cell(row + 5, 7).Value = item.Mandrel ?? "N/A";
                    ws.Cell(row + 5, 8).Value = item.StepName ?? "N/A";
                    ws.Cell(row + 5, 9).Value = item.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                    ws.Cell(row + 5, 10).Value = item.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
                    
                    if (item.StartedAt.HasValue && item.EndedAt.HasValue)
                    {
                        var duration = item.EndedAt.Value - item.StartedAt.Value;
                        ws.Cell(row + 5, 11).Value = duration.ToString(@"hh\:mm\:ss");
                    }
                    else if (item.StartedAt.HasValue)
                    {
                        ws.Cell(row + 5, 11).Value = "Đang chạy...";
                    }
                    else
                    {
                        ws.Cell(row + 5, 11).Value = "N/A";
                    }
                    
                    ws.Cell(row + 5, 12).Value = item.Work ?? "N/A";

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
