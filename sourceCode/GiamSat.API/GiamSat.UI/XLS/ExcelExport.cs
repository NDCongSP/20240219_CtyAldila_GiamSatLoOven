using ClosedXML.Excel;
using ClosedXML.Report;
using GiamSat.APIClient;
using MoreLinq;

namespace GiamSat.UI
{
    public class ExcelExport
    {
        public byte[] GenerateExcelFile(List<FT03> data)
        {

            List<ExcelModel> model = new List<ExcelModel>();
            foreach (var item in data)
            {
                model.Add(new ExcelModel()
                {
                    OvenId=item.OvenId,
                    Ovenname=item.OvenName,
                    Temperature=item.Temperature,
                    CreatedDate=item.CreatedDate
                });
            }

            using (var wb = new XLWorkbook())
            {
                wb.Properties.Author = "the Author";
                wb.Properties.Title = "the Title";
                wb.Properties.Subject = "the Subject";
                wb.Properties.Category = "the Category";
                wb.Properties.Keywords = "the Keywords";
                wb.Properties.Comments = "the Comments";
                wb.Properties.Status = "the Status";
                wb.Properties.LastModifiedBy = "the Last Modified By";
                wb.Properties.Company = "the Company";
                wb.Properties.Manager = "the Manager";

                var ws = wb.Worksheets.Add("Weather Forecast");

                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = "Oven";
                ws.Cell(1, 3).Value = "Nhiệt độ (oC)";
                ws.Cell(1, 4).Value = "Thời gian";

                // Fill a cell with a date
                var wRange = ws.Range($"D1:D{data.Count}");
                wRange.Style.DateFormat.Format = "yyyy-MMM-dd HH:mm:ss";

                var row = 0;
                foreach (var item in model)
                {
                    // The apostrophe is to force ClosedXML to treat the date as a string
                    //thay chi tiet cac cot data vao bem duoi.
                    ws.Cell(row + 1, 1).Value = item.OvenId;
                    ws.Cell(row + 1, 2).Value = item.Ovenname;
                    ws.Cell(row + 1, 3).Value = item.Temperature;
                    ws.Cell(row + 1, 4).Value = item.CreatedDate;

                    row += 1;
                }

                var bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    bytes = ms.ToArray();
                }

                return bytes;
            }
        }

        public byte[] ExcelTemplate(Stream streamTemplate, List<FT04> data)
        {
            var d = new List<ExcelModel>();

            foreach (var item in data)
            {
                d.Add(new ExcelModel()
                {
                    //CreatedDate = item.CreatedDate,
                    //TenChuong = item.TenChuong,
                    //NhietDo = item.NhietDo,
                    //DoAm = item.DoAm,
                    //Frequency = item.Frequency,
                });
            }

            var template = new XLTemplate(streamTemplate);


            template.AddVariable("DataLogModel", d);
            template.Generate();

            MemoryStream XLSStream = new();
            template.SaveAs(XLSStream);

            return XLSStream.ToArray();
        }


        public async Task<byte[]> FillIn(IHttpClientFactory client, List<FT04> data, string template,string dateTime)
        {
            // Open existing XLS
            var mdFile = await client.CreateClient("local").GetByteArrayAsync(template);
            Stream stream = new MemoryStream(mdFile);

            XLWorkbook wb = new XLWorkbook(stream);

            var ws = wb.Worksheet("BaoCao");

            var row = 0;
            foreach (var item in data)
            {
                // The apostrophe is to force ClosedXML to treat the date as a string
                ws.Cell(row + 4, 1).Value = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cell(row + 4, 2).Value = item.OvenName;
                ws.Cell(row + 4, 3).Value = item.Temperature;
                ws.Cell(row + 4, 4).Value = item.Setpoint;
                ws.Cell(row + 4, 5).Value = item.StepName;

                row += 1;
            }

            var rowCount = data.Count;

            ws.Cell("A2").Value = $"Thời gian: {dateTime}";

            ws.Range($"C4:D{data.Count + 3}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .NumberFormat.Format = "#,##0.00";

            ws.Range($"A4:A{data.Count + 3}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
               .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
               .DateFormat.Format = "yyyy-MM-dd HH:mm:ss";

            ws.Range($"A2:E{data.Count + 3}").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            MemoryStream XLSStream = new();
            wb.SaveAs(XLSStream);

            return XLSStream.ToArray();
        }
    }

    class ExcelModel
    { 
        public int OvenId { get; set; }
        public string Ovenname { get; set; }
        public double? Temperature { get; set; }        
        public DateTime CreatedDate { get; set; }
    }
}
