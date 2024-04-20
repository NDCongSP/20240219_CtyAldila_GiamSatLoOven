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
            OvenId = 0
        };

        Radzen.Blazor.RadzenChart RadzenChartDataLog = new Radzen.Blazor.RadzenChart();
        List<DataItem> _chartDataSeriesTempDataLog = new List<DataItem>();
        List<DataItem> _chartDataSeriesSetpointDataLog = new List<DataItem>();

        OvensInfo _ovensInfo;

        List<APIClient.FT04> _dataProfile = new List<APIClient.FT04>();
        Radzen.Blazor.RadzenChart RadzenChartProfifle = new Radzen.Blazor.RadzenChart();
        List<DataItem> _chartDataSeriesTempProfile = new List<DataItem>();
        List<DataItem> _chartDataSeriesSetpointProfile = new List<DataItem>();
        List<DataItem> _chartDataSeriesLevelUpProfile = new List<DataItem>();
        List<DataItem> _chartDataSeriesLevelDownProfile = new List<DataItem>();
        APIClient.FilterModel _filterProfileLog = new APIClient.FilterModel()
        {
            GetAll = false,
            FromDate = DateTime.Now,
            ToDate = DateTime.Now,
            OvenId = 0
        };

        RadzenDataGrid<APIClient.FT04> _dataProfileGrid;

        RadzenDropDown<int> radzenDropDown;
        List<DropDownModel> _dropDownData = new List<DropDownModel>();

        //inject theo ten.
        [Inject]
        public IHttpClientFactory _client { get; set; }

        bool _showProgressBar = false;

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
                        Duration = 2000
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

                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {

                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 2000
                });
                return;
            }
        }

        async Task QueryData()
        {
            try
            {
                if (_filterModelDataLog.OvenId == 0)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Warning",
                        Detail = "Bạn chưa chọn lò cần truy vấn.",
                        Duration = 2000
                    });
                    return;
                }

                if (_filterModelDataLog.OvenId > _ovensInfo.Count)
                {
                    _filterModelDataLog.GetAll = true;
                }
                else
                {
                    _filterModelDataLog.GetAll = false;
                }

                _showProgressBar = true;

                var res = await _ft03Client.GetFilterAsync(_filterModelDataLog);

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new Radzen.NotificationMessage()
                    {
                        Severity = Radzen.NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Truy cập API lỗi.",
                        Duration = 2000
                    });

                    _showProgressBar = false;
                    InvokeAsync(StateHasChanged);
                    return;
                }

                _dataReport = new List<APIClient.FT03>();
                _chartDataSeriesTempDataLog = new List<DataItem>();
                _chartDataSeriesSetpointDataLog = new List<DataItem>();

                _dataReport = res.Data.ToList();

                UpdateDataSeriesChartDataLog(_dataReport.OrderBy(x => x.CreatedDate).ToList(), _filterModelDataLog.GetAll);

                await RadzenChartDataLog.Reload();

                _showProgressBar = false;
                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 2000
                });

                _showProgressBar = false;
                InvokeAsync(StateHasChanged);
                return;
            }
        }

        async Task QueryDataProfile()
        {
            try
            {
                if (_filterProfileLog.OvenId == 0)
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Warning",
                        Detail = "Bạn chưa chọn lò cần truy vấn.",
                        Duration = 2000
                    });
                    return;
                }

                _dataProfile = new List<APIClient.FT04>();
                _chartDataSeriesTempProfile = new List<DataItem>();
                _chartDataSeriesSetpointProfile = new List<DataItem>();
                _chartDataSeriesLevelUpProfile = new List<DataItem>();
                _chartDataSeriesLevelDownProfile = new List<DataItem>();

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

                _showProgressBar = true;

                var res = await _ft04Client.GetFilterAsync(_filterProfileLog);

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new Radzen.NotificationMessage()
                    {
                        Severity = Radzen.NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Truy cập API lỗi.",
                        Duration = 2000
                    });
                    _showProgressBar = false;
                    return;
                }

                _dataProfile = res.Data.OrderBy(x => x.CreatedDate).ToList();

                UpdateDataSeriesChartProfile(_dataProfile);

                await RadzenChartProfifle.Reload();

                _showProgressBar = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 2000
                });
                _showProgressBar = false;

                InvokeAsync(StateHasChanged);
                return;
            }
        }

        async Task ExportProfileAsync()
        {
            try
            {
                _showProgressBar = true;
                var xls = new Excel();
                //await xls.GenerateExcel(_js, Elements, "export.xlsx");

                //Stream streamTemplate = await _client.CreateClient("local").GetStreamAsync("templateXLS/TemplateReport.xlsx");
                //await xls.UseTemplate(_js, streamTemplate, Elements, "BaoCao.xlsx");

                await xls.TemplateOnExistingFileAsync(_client, _js, _dataProfile, @"templateXLS\TemplateReport.xlsx"
                                    , $"{_filterProfileLog.FromDate} đến {_filterProfileLog.ToDate}", $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_ReportRunProfile.xlsx");

                _showProgressBar = false;
                InvokeAsync(StateHasChanged);
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
                _showProgressBar = false;
                InvokeAsync(StateHasChanged);
                return;
            }
        }

        async Task ExportDataLogAsync()
        {
            try
            {
                _showProgressBar = true;
                var xls = new Excel();
                //await xls.GenerateExcel(_js, Elements, "export.xlsx");

                //Stream streamTemplate = await _client.CreateClient("local").GetStreamAsync("templateXLS/TemplateReport.xlsx");
                //await xls.UseTemplate(_js, streamTemplate, Elements, "BaoCao.xlsx");

                await xls.GenerateExcel(_js, _dataReport.OrderBy(x => x.CreatedDate).ToList(), $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_ReportDataLog.xlsx"
                    , $"{_filterModelDataLog.FromDate} đến {_filterModelDataLog.ToDate}");

                _showProgressBar = false;
                InvokeAsync(StateHasChanged);
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

                _showProgressBar = false;
                InvokeAsync(StateHasChanged);
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
                return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return string.Empty;
        }

        void UpdateDataSeriesChartProfile(List<APIClient.FT04> data)
        {
            foreach (var item in data)
            {
                var detail = JsonConvert.DeserializeObject<RealtimeDisplayModel>(item.Details);
                _chartDataSeriesTempProfile.Add(new DataItem()
                {
                    Temperature = item.Temperature,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                _chartDataSeriesSetpointProfile.Add(new DataItem
                {
                    Temperature = item.Setpoint,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                _chartDataSeriesLevelUpProfile.Add(new DataItem
                {
                    Temperature = detail.LevelUp,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                _chartDataSeriesLevelDownProfile.Add(new DataItem
                {
                    Temperature = detail.LevelDown,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        /// <summary>
        /// Tao series cho chart.
        /// </summary>
        /// <param name="data">data log.</param>
        /// <param name="filterMode">filter mode. false-1 lo; true-All.</param>
        void UpdateDataSeriesChartDataLog(List<APIClient.FT03> data, bool filterMode)
        {
            foreach (var item in data)
            {
                Models.RealtimeDisplayModel detail = new Models.RealtimeDisplayModel();
                if (!string.IsNullOrEmpty(item.Details))
                {
                    detail = JsonConvert.DeserializeObject<Models.RealtimeDisplayModel>(item.Details);
                }

                _chartDataSeriesTempDataLog.Add(new DataItem()
                {
                    Temperature = item.Temperature,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                _chartDataSeriesSetpointDataLog.Add(new DataItem
                {
                    Temperature = detail != null ? detail.SetPoint : 0,
                    Date = item.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
    }
}
