using ClosedXML.Excel;
using ClosedXML.Report;
using GiamSat.APIClient;
using GiamSat.Models;
using MoreLinq;
using Newtonsoft.Json;
using System.Drawing;

namespace GiamSat.UI
{
    public class ExcelExport
    {
        public byte[] GenerateExcelFile(List<APIClient.FT03> data, string dateQuery)
        {

            List<ExcelModel> model = new List<ExcelModel>();
            foreach (var item in data)
            {
                Models.RealtimeDisplayModel detail = new Models.RealtimeDisplayModel();
                if (!string.IsNullOrEmpty(item.Details))
                {
                    detail = JsonConvert.DeserializeObject<Models.RealtimeDisplayModel>(item.Details);
                }

                model.Add(new ExcelModel()
                {
                    OvenId = item.OvenId,
                    Ovenname = item.OvenName,
                    Temperature = item.Temperature,
                    CreatedDate = item.CreatedDate,
                    ProfileName = detail.ProfileName,
                    StepName = detail.StepName.ToString(),
                    AlarmDescription = detail?.AlarmDescription,
                    SetPoint = detail?.SetPoint,
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

                var ws = wb.Worksheets.Add("DataLog");

                ws.Range(1, 1, 1, 8).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Font.SetFontSize(15).Font.SetBold(true)
                ;

                ws.Range(2, 1, 2, 8).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
               .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                ws.Cell(1, 1).Value = "REPORT";
                ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.Orange;
                ws.Cell(2, 1).Value = $"Thời gian: {dateQuery}";

                ws.Cell(3, 1).Value = "Thời gian";
                ws.Cell(3, 2).Value = "Id";
                ws.Cell(3, 3).Value = "Oven";
                ws.Cell(3, 4).Value = "Nhiệt độ đặt (oC)";
                ws.Cell(3, 5).Value = "Nhiệt độ (oC)";
                ws.Cell(3, 6).Value = "Profile";
                ws.Cell(3, 7).Value = "Step";
                ws.Cell(3, 8).Value = "Cảnh báo";

                ws.Range(3, 1, 3, 8).SetAutoFilter(true);
                ws.Range(3, 1, 3, 8).Style.Fill.BackgroundColor = XLColor.LightCyan;

                // Fill a cell with a date
                ws.Range($"A3:A{data.Count + 3}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                                   .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                                   .DateFormat.Format = "yyyy-MM-dd HH:mm:ss";

                ws.Range($"A3:H{data.Count + 3}").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                var row = 0;
                foreach (var item in model)
                {
                    // The apostrophe is to force ClosedXML to treat the date as a string
                    //thay chi tiet cac cot data vao bem duoi.
                    ws.Cell(row + 4, 1).Value = item.CreatedDate;
                    ws.Cell(row + 4, 2).Value = item.OvenId;
                    ws.Cell(row + 4, 3).Value = item.Ovenname;
                    ws.Cell(row + 4, 4).Value = item.SetPoint;
                    ws.Cell(row + 4, 5).Value = item.Temperature;
                    ws.Cell(row + 4, 6).Value = item.ProfileName;
                    ws.Cell(row + 4, 7).Value = item.StepName;
                    ws.Cell(row + 4, 8).Value = item.AlarmDescription;

                    row += 1;
                }

                ws.Columns().AdjustToContents();//Adjust Row Height and Column Width to Contents

                var bytes = new byte[0];
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    bytes = ms.ToArray();
                }

                return bytes;
            }
        }

        public byte[] ExcelTemplate(Stream streamTemplate, List<APIClient.FT04> data)
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


        public async Task<byte[]> FillIn(IHttpClientFactory client, List<APIClient.FT04> data, string template, string dateTime)
        {
            // Open existing XLS
            var mdFile = await client.CreateClient("local").GetByteArrayAsync(template);
            Stream stream = new MemoryStream(mdFile);

            XLWorkbook wb = new XLWorkbook(stream);

            var ws = wb.Worksheet("BaoCao");

            var row = 0;
            foreach (var item in data)
            {
                Models.RealtimeDisplayModel detail = new Models.RealtimeDisplayModel();
                if (!string.IsNullOrEmpty(item.Details))
                {
                    detail = JsonConvert.DeserializeObject<Models.RealtimeDisplayModel>(item.Details);
                }
                // The apostrophe is to force ClosedXML to treat the date as a string
                ws.Cell(row + 4, 1).Value = item.CreatedDate;
                ws.Cell(row + 4, 2).Value = item.OvenName;
                ws.Cell(row + 4, 3).Value = item.Setpoint;
                ws.Cell(row + 4, 4).Value = item.Temperature;
                ws.Cell(row + 4, 5).Value = item.ProfileName;
                ws.Cell(row + 4, 6).Value = item.StepName;
                ws.Cell(row + 4, 7).Value = detail?.AlarmDescription;
                ws.Cell(row + 4, 8).Value = item.StartTime;
                ws.Cell(row + 4, 9).Value = item.EndTime;

                row += 1;
            }

            var rowCount = data.Count;

            ws.Cell("A2").Value = $"Thời gian: {dateTime}";

            ws.Range($"A3:I3").SetAutoFilter(true);

            ws.Range($"C4:D{data.Count + 4}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .NumberFormat.Format = "#,##0.00";

            ws.Range($"B4:B{data.Count + 4}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
               .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Range($"E4:G{data.Count + 4}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
         .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            ws.Range($"A4:A{data.Count + 4}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
               .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
               .DateFormat.Format = "yyyy-MM-dd HH:mm:ss";

            ws.Range($"H4:I{data.Count + 4}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
             .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
             .DateFormat.Format = "yyyy-MM-dd HH:mm:ss";

            ws.Range($"A3:I{data.Count + 3}").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                                       .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            MemoryStream XLSStream = new();
            wb.SaveAs(XLSStream);

            return XLSStream.ToArray();
        }
    }

    class ExcelModel
    {
        public DateTime CreatedDate { get; set; }
        public int OvenId { get; set; }
        public string Ovenname { get; set; }
        public double? SetPoint { get; set; }
        public double? Temperature { get; set; }
        public string ProfileName { get; set; }
        public string StepName { get; set; }
        public string AlarmDescription { get; set; }
    }
}
