using MudBlazor;
using static MudBlazor.CategoryTypes;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using GiamSat.APIClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ClosedXML.Excel;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http;


namespace GiamSat.UI.Pages
{
    public partial class Report
    {
        private MudTheme _theme = new MudTheme();

        //tạo các biến dùng cho tạo select chọn giai doạn
        private string stringValue { get; set; }
        private ChuongEnum enumValue { get; set; } = ChuongEnum.All;
        public enum ChuongEnum { Chuong1 = 1, Chuong2 = 2, Chuong3 = 3, Chuong4 = 4, All = 5 }

        DateTime? _dateFrom = DateTime.Today;
        DateTime? _dateTo = DateTime.Today;


        private bool _loading = true;
        private string searchString1 = "";
        private APIClient.DataLogModel selectedItem1 = null;
        private List<APIClient.DataLogModel> Elements;

        private string _tenChuong = "All";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var res = await _dataLogClient.GetFromToByNameAsync(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"), DateTime.Now.ToString("yyyy-MM-dd 23:59:59"), "All");
                if (res.Succeeded)
                {
                    Elements = res.Data.OrderByDescending(x => x.CreatedDate).ToList();
                }

                _loading = false;
            }
            catch (Exception ex) { _snackBar.Add(ex.Message, Severity.Error); }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _loading = false;
            }
        }

        private void OnSelectedValuesChanged(IEnumerable<ChuongEnum> values)
        {
            Console.WriteLine(string.Join("SELECTED value:\n", values));

            var chuong = values.FirstOrDefault();

            if (chuong.ToString() == "Chuong1")
            {
                _tenChuong = "Chuồng 1";
            }
            else if (chuong.ToString() == "Chuong2")
            {
                _tenChuong = "Chuồng 2";
            }
            else if (chuong.ToString() == "Chuong3")
            {
                _tenChuong = "Chuồng 3";
            }
            else if (chuong.ToString() == "Chuong4")
            {
                _tenChuong = "Chuồng 4";
            }
            else
            {
                _tenChuong = "All";
            }

            //var s = enumValue;

            StateHasChanged();
        }

        private bool FilterFunc1(APIClient.DataLogModel element) => FilterFunc(element, searchString1);

        private bool FilterFunc(APIClient.DataLogModel element, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.TenChuong.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private async void OnQueryClick()
        {
            try
            {
                //var Chuong = enumValue.ToString();

                var res = await _dataLogClient.GetFromToByNameAsync(_dateFrom?.ToString("yyyy-MM-dd 00:00:00"), _dateTo?.ToString("yyyy-MM-dd 23:59:59"), _tenChuong);
                if (res.Succeeded)
                {
                    Elements = res.Data.OrderByDescending(x => x.CreatedDate).ToList();
                }

                StateHasChanged();
            }
            catch (Exception ex) { _snackBar.Add(ex.Message, Severity.Error); }
        }

        //inject theo ten.
        [Inject]
        public IHttpClientFactory _client { get; set; }

        private async void OnExportExcelClick()
        {
            try
            {
                var xls = new Excel();
                //await xls.GenerateExcel(_js, Elements, "export.xlsx");
                
                //Stream streamTemplate = await _client.CreateClient("local").GetStreamAsync("templateXLS/TemplateReport.xlsx");

                //await xls.UseTemplate(_js, streamTemplate, Elements, "BaoCao.xlsx");
                await xls.TemplateOnExistingFileAsync(_client,_js, Elements, @"templateXLS\TemplateReport.xlsx",$"{_dateFrom} đến {_dateTo}");
            }
            catch (Exception ex) { _snackBar.Add(ex.Message, Severity.Error); }
        }
    }
}
