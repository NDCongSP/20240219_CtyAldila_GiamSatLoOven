//using DocumentFormat.OpenXml.InkML;
using GiamSat.APIClient;
using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ClosedXML.Excel;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http;

namespace GiamSat.UI.Pages
{
    public partial class Report
    {
        private List<APIClient.FT03> _dataReport = new List<APIClient.FT03>();

        RadzenDataGrid<APIClient.FT03> _dataReportGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 10, 50, 100, 200 };

        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";
        APIClient.FilterModel _filterModelDataLog = new APIClient.FilterModel()
        {
            GetAll = false,
            FromDate = DateTime.Now,
            ToDate = DateTime.Now,
        };
        OvensInfo _ovensInfo;

        List<APIClient.FT04> _dataProfile = new List<APIClient.FT04>();
        Radzen.Blazor.RadzenChart RadzenChart = new Radzen.Blazor.RadzenChart();
        List<DataItem> _chartDataSeriesTemp = new List<DataItem>();
        List<DataItem> _chartDataSeriesSetpoint = new List<DataItem>();
        APIClient.FilterModel _filterProfileLog = new APIClient.FilterModel()
        {
            GetAll = false,
            FromDate = DateTime.Now,
            ToDate = DateTime.Now
        };

        RadzenDataGrid<APIClient.FT04> _dataProfileGrid;

        RadzenDropDown<int> radzenDropDown;
        List<DropDownModel> _dropDownData = new List<DropDownModel>();

        //inject theo ten.
        [Inject]
        public IHttpClientFactory _client { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                var res = await _ft01Client.GetAllAsync();
                if (!res.Succeeded)
                {
                    _notificationService.Notify(new Radzen.NotificationMessage()
                    {
                        Severity = Radzen.NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Truy cập API lỗi.",
                        Duration = 4000
                    });
                    return;
                }

                var ft01 = res.Data.ToList();
                _ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(ft01.FirstOrDefault().C001);

                foreach (var item in _ovensInfo)
                {
                    _dropDownData.Add(new DropDownModel()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                    });
                }

                _dropDownData.Add(new DropDownModel()
                {
                    OvenId = _ovensInfo.Count + 1,
                    OvenName = "All"
                });
            }
            catch (Exception ex)
            {

                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 4000
                });
                return;
            }
        }

        async void QueryData()
        {
            try
            {
                if (_filterModelDataLog.OvenId > _ovensInfo.Count)
                {
                    _filterModelDataLog.GetAll = true;
                }
                else
                {
                    _filterModelDataLog.GetAll = false;
                }

                var res = await _ft03Client.GetFilterAsync(_filterModelDataLog);

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new Radzen.NotificationMessage()
                    {
                        Severity = Radzen.NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Truy cập API lỗi.",
                        Duration = 4000
                    });
                    return;
                }

                _dataReport = res.Data.ToList();

                InvokeAsync(StateHasChanged);

            }
            catch (Exception ex)
            {
                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 4000
                });
                return;
            }

        }

        async void QueryDataProfile()
        {
            try
            {
                _dataProfile = null;
                _dataProfile = new List<APIClient.FT04>();
                _chartDataSeriesTemp = null;
                _chartDataSeriesTemp = new List<DataItem>();
                _chartDataSeriesSetpoint = null;
                _chartDataSeriesSetpoint = new List<DataItem>();

                if (_filterProfileLog.OvenId > _ovensInfo.Count)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Cảnh báo",
                        Detail = "Không được chọn xem tất cả ở chức năng này.",
                        Duration = 3000
                    });
                    return;
                }
                else
                {
                    _filterProfileLog.GetAll = false;
                }

                var res = await _ft04Client.GetFilterAsync(_filterProfileLog);

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new Radzen.NotificationMessage()
                    {
                        Severity = Radzen.NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Truy cập API lỗi.",
                        Duration = 4000
                    });
                    return;
                }

                _dataProfile = res.Data.ToList();

                UpdateDataSeriesChart(_dataProfile.OrderBy(x => x.CreatedDate).ToList());

                await RadzenChart.Reload();

                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 4000
                });
                return;
            }

        }

        async Task ExportProfileAsync()
        {
            try
            {
                var xls = new Excel();
                //await xls.GenerateExcel(_js, Elements, "export.xlsx");

                //Stream streamTemplate = await _client.CreateClient("local").GetStreamAsync("templateXLS/TemplateReport.xlsx");
                //await xls.UseTemplate(_js, streamTemplate, Elements, "BaoCao.xlsx");

                await xls.TemplateOnExistingFileAsync(_client, _js, _dataProfile, @"templateXLS\TemplateReport.xlsx", $"{_filterProfileLog.FromDate} đến {_filterProfileLog.ToDate}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 3000
                });
                return;
            }
        }

        /// <summary>
        /// Tab select event.
        /// </summary>
        /// <param name="index"></param>
        void OnChangeTab(int index)
        {
            Console.WriteLine($"Tab with index {index} was selected.");
        }

        string FormatAsY(object value)
        {
            return ((double)value).ToString();
        }

        string FormatAsX(object value)
        {
            if (value != null)
            {
                return Convert.ToDateTime(value).ToString("HH:mm:ss");
            }

            return string.Empty;
        }

        void UpdateDataSeriesChart(List<APIClient.FT04> data)
        {
            foreach (var item in data)
            {
                _chartDataSeriesTemp.Add(new DataItem()
                {
                    Temperature = item.Temperature,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                _chartDataSeriesSetpoint.Add(new DataItem
                {
                    Temperature = item.Setpoint,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
    }
}
