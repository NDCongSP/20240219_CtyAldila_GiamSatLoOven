using GiamSat.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GiamSat.API.Services
{
    public class PdfExportRevo
    {
        private static bool IsAutoRolling(FT09_RevoDatalog row)
        {
            var revo = row.RevoName ?? string.Empty;
            var work = row.Work ?? string.Empty;
            return revo.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || work.Contains("auto rolling", StringComparison.OrdinalIgnoreCase)
                || revo.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase)
                || work.Replace(" ", string.Empty).Contains("autorolling", StringComparison.OrdinalIgnoreCase);
        }

        public byte[] GeneratePdfFile(List<FT09_RevoDatalog> data, string dateQuery)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Background(Colors.Green.Darken1)
                        .Padding(10)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text("BÁO CÁO REVO")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.White);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Thông tin thời gian
                            column.Item()
                                .AlignCenter()
                                .Text($"Thời gian: {dateQuery}")
                                .FontSize(12)
                                .Bold();

                            // Summary: Shaft count + Record count (đưa lên trên đầu)
                            var shaftCount = data.Select(x => x.ShaftNum).Where(x => x.HasValue).Distinct().Count();

                            column.Item()
                                .Background(Colors.Blue.Lighten5)
                                .Padding(8)
                                .Row(row =>
                                {
                                    row.AutoItem().Text($"Tổng số Shaft: {shaftCount}").Bold().FontSize(10);
                                    row.AutoItem().PaddingHorizontal(10).Text("|").FontSize(10);
                                    row.AutoItem().Text($"Tổng số bản ghi: {data.Count}").Bold().FontSize(10);
                                });

                            // Bảng dữ liệu
                            column.Item()
                                .Table(table =>
                                {
                                    // Header
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.2f); // Tên REVO
                                        columns.RelativeColumn(1.8f); // ShaftNum
                                        columns.RelativeColumn(0.6f); // StepId
                                        columns.RelativeColumn(1.0f); // Part
                                        columns.RelativeColumn(0.8f); // Rev
                                        columns.RelativeColumn(0.8f); // Màu
                                        columns.RelativeColumn(1.0f); // Mandrel
                                        columns.RelativeColumn(1.5f); // Tên Step
                                        columns.RelativeColumn(1.3f); // Thời gian bắt đầu
                                        columns.RelativeColumn(1.3f); // Thời gian kết thúc
                                        columns.RelativeColumn(1.0f); // Thời lượng
                                        columns.RelativeColumn(1.0f); // Work
                                    });

                                    // Header row
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Tên REVO").Bold();
                                        header.Cell().Element(CellStyle).Text("ShaftNum").Bold();
                                        header.Cell().Element(CellStyle).Text("StepId").Bold();
                                        header.Cell().Element(CellStyle).Text("Part").Bold();
                                        header.Cell().Element(CellStyle).Text("Rev").Bold();
                                        header.Cell().Element(CellStyle).Text("Màu").Bold();
                                        header.Cell().Element(CellStyle).Text("Mandrel").Bold();
                                        header.Cell().Element(CellStyle).Text("Tên Step").Bold();
                                        header.Cell().Element(CellStyle).Text("TG bắt đầu").Bold();
                                        header.Cell().Element(CellStyle).Text("TG kết thúc").Bold();
                                        header.Cell().Element(CellStyle).Text("Thời lượng").Bold();
                                        header.Cell().Element(CellStyle).Text("Work").Bold();

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container
                                                .Background(Colors.Grey.Lighten3)
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten1)
                                                .PaddingVertical(5)
                                                .PaddingHorizontal(5);
                                        }
                                    });

                                    // Data rows
                                    foreach (var item in data)
                                    {
                                        var isAutoRolling = IsAutoRolling(item);
                                        var durationText = "N/A";
                                        if (isAutoRolling)
                                        {
                                            var total = item.TotalTime ?? 0;
                                            durationText = total > 0
                                                ? TimeSpan.FromSeconds(total).ToString(@"hh\:mm\:ss")
                                                : "N/A";
                                        }
                                        else if (item.StartedAt.HasValue && item.EndedAt.HasValue)
                                        {
                                            var duration = item.EndedAt.Value - item.StartedAt.Value;
                                            durationText = duration.ToString(@"hh\:mm\:ss");
                                        }
                                        else if (item.StartedAt.HasValue)
                                        {
                                            durationText = "Đang chạy...";
                                        }

                                        table.Cell().Element(CellStyle).Text(item.RevoName ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.ShaftNum?.ToString() ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.StepId?.ToString() ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.Part ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.Rev ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.ColorCode ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.Mandrel ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.StepName ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(isAutoRolling ? "N/A" : item.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(isAutoRolling ? "N/A" : item.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(durationText);
                                        table.Cell().Element(CellStyle).Text(item.Work ?? "N/A");

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(3)
                                                .PaddingHorizontal(5);
                                        }
                                    }
                                });

                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.DefaultTextStyle(TextStyle.Default.FontSize(9).FontColor(Colors.Grey.Medium));
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
