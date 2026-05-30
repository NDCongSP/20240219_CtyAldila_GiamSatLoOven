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
        public async Task GenerateExcel(IJSRuntime js, List<FT03> data, string filename = "export.xlsx", string dateQuery = null)
        {
            var exportXls = new ExcelExport();
            var XLSStream =await exportXls.GenerateExcelFileAsync(data, dateQuery);

            await js.InvokeVoidAsync("BlazorDownloadFile", filename, XLSStream);
        }

        public async Task UseTemplate(IJSRuntime js,
                                                       Stream streamTemplate,
                                                      List<FT04> data,
                                                       string filename = "TemplateReport.xlsx")
        {
            var templateXLS = new ExcelExport();
            var XLSStream = templateXLS.ExcelTemplate(streamTemplate, data);

            await js.InvokeVoidAsync("BlazorDownloadFile", filename, XLSStream);
        }


        public async Task TemplateOnExistingFileAsync(IHttpClientFactory client,
                                                      IJSRuntime js,
                                                      List<FT04> data,
                                                      string template,
                                                      string dateTime
            , string filename)
        {
            var templateXLS = new ExcelExport();
            var XLSStream = await templateXLS.FillInAsync(client, data, template, dateTime);

            await js.InvokeVoidAsync("BlazorDownloadFile", filename, XLSStream);
        }
    }
}
