﻿using ClosedXML.Excel;
using ClosedXML.Report;
using DocumentFormat.OpenXml.Spreadsheet;
using GiamSat.APIClient;
using System;

namespace GiamSat.UI
{
    public class UseTemplateXLS
    {

        public byte[] Edition(Stream streamTemplate, List<FT04> data)
        {
            var template = new XLTemplate(streamTemplate);

            template.AddVariable("WeatherForecasts", data);
            template.Generate();

            MemoryStream XLSStream = new();
            template.SaveAs(XLSStream);


            return XLSStream.ToArray();
        }

        public async Task<byte[]> FillIn(HttpClient client, Stream streamTemplate, List<FT04> data, string existingXLS)
        {
            // Open the Templace
            var template = new XLTemplate(streamTemplate);
            // Send Data
            template.AddVariable("WeatherForecasts", data);

            template.Generate();

            var sheetCopied = template.Workbook.Worksheet(1);

            //if (wb.TryGetWorksheet("F", out var ws))
            //{
            //    ws.Value = template.Generate;
            //}
            // Open existing XLS
            var mdFile = await client.GetByteArrayAsync(existingXLS);
            Stream stream = new MemoryStream(mdFile);

            XLWorkbook wb = new XLWorkbook(stream);
            wb.AddWorksheet(sheetCopied);


            MemoryStream XLSStream = new();
            wb.SaveAs(XLSStream);


            return XLSStream.ToArray();
        }
    }
}
