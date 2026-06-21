using GiamSat.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using GiamSat.API.Services.ExportWorker;

namespace GiamSat.API.Services
{
    public class PdfExportRevo
    {
        public byte[] GeneratePdfFile(IReadOnlyList<RevoStepRow> rows, string dateQuery, int totalShafts)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
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

                            column.Item()
                                .Background(Colors.Blue.Lighten5)
                                .Padding(8)
                                .Row(row =>
                                {
                                    row.AutoItem().Text($"Tổng số Shaft: {totalShafts}").Bold().FontSize(10);
                                    row.AutoItem().PaddingHorizontal(10).Text("|").FontSize(10);
                                    row.AutoItem().Text($"Tổng số bản ghi: {rows.Count}").Bold().FontSize(10);
                                });

                            // Bảng dữ liệu
                            column.Item()
                                .Table(table =>
                                {
                                    // Header
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(0.6f); // STT
                                        columns.RelativeColumn(1.8f); // Tên REVO
                                        columns.RelativeColumn(1.2f); // Shaft
                                        columns.RelativeColumn(1.0f); // Part
                                        columns.RelativeColumn(0.8f); // Rev
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
                                        header.Cell().Element(CellStyle).Text("STT").Bold();
                                        header.Cell().Element(CellStyle).Text("Tên REVO").Bold();
                                        header.Cell().Element(CellStyle).Text("Shaft").Bold();
                                        header.Cell().Element(CellStyle).Text("Part").Bold();
                                        header.Cell().Element(CellStyle).Text("Rev").Bold();
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
                                    foreach (var item in rows)
                                    {
                                        table.Cell().Element(CellStyle).Text(item.Stt.ToString());
                                        table.Cell().Element(CellStyle).Text(item.RevoName ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.ShaftKey ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.Part ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.Rev ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.Mandrel ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.StepDisplay ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.StartedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.EndedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(item.DurationText ?? "N/A");
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
