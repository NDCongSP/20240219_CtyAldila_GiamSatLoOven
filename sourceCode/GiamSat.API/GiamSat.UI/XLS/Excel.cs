using GiamSat.APIClient;
using Microsoft.JSInterop;

namespace GiamSat.UI
{
    public class Excel
    {
        /// <summary>
        /// Generate excel file.
        /// </summary>
        /// <param name="js"></param>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task GenerateExcel(IJSRuntime js, List<DataLogModel> data, string filename = "export.xlsx")
        {
            var exportXls = new ExcelExport();
            var XLSStream = exportXls.GenerateExcelFile(data);

            await js.InvokeVoidAsync("BlazorDownloadFile", filename, XLSStream);
        }

        public async Task UseTemplate(IJSRuntime js,
                                                       Stream streamTemplate,
                                                      List<DataLogModel> data,
                                                       string filename = "TemplateReport.xlsx")
        {
            var templateXLS = new ExcelExport();
            var XLSStream = templateXLS.ExcelTemplate(streamTemplate, data);

            await js.InvokeVoidAsync("BlazorDownloadFile", filename, XLSStream);
        }


        public async Task TemplateOnExistingFileAsync(IHttpClientFactory client,
                                                      IJSRuntime js,
                                                      List<DataLogModel> data,
                                                      string template,
                                                      string dateTime)
        {
            var templateXLS = new ExcelExport();
            var XLSStream = await templateXLS.FillIn(client, data, template, dateTime);

            await js.InvokeVoidAsync("BlazorDownloadFile", "export.xlsx", XLSStream);
        }
    }
}
