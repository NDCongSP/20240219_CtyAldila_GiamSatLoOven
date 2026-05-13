using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GiamSat.UI.Pages
{
    public partial class TemperatureReport
    {
        [Inject] private ITemperatureDataClient _temperatureDataClient { get; set; } = default!;

        private RadzenDataGrid<FT13_TemperatureAlarmLog> _dataGrid = default!;
        private ICollection<FT13_TemperatureAlarmLog> _logs = new List<FT13_TemperatureAlarmLog>();

        private RadzenDataGrid<FT12_TemperatureDatalog> _dataGridLog = default!;
        private ICollection<FT12_TemperatureDatalog> _dataLogs = new List<FT12_TemperatureDatalog>();
        
        private DateTime _fromDate = DateTime.Today;
        private DateTime _toDate = DateTime.Today;
        private bool _isLoading = false;
        private int _tabIndex = 0;

        private bool IsExportDisabled => _tabIndex == 0 ? (_logs == null || !_logs.Any()) : (_dataLogs == null || !_dataLogs.Any());

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
             _isLoading = true;
            try
            {
                if (_tabIndex == 0)
                {
                    var result = await _temperatureDataClient.GetAlarmLogsAsync(_fromDate, _toDate);
                    _logs = result?.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.LocationName).ToList() ?? new List<FT13_TemperatureAlarmLog>();
                }
                else
                {
                    var result = await _temperatureDataClient.GetDataLogsAsync(_fromDate, _toDate);
                    _dataLogs = result?.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.LocationName).ToList() ?? new List<FT12_TemperatureDatalog>();
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi Server", $"Không thể tải báo cáo: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task ExportExcel()
        {
            try
            {
                _isLoading = true;
                var export = new ExcelExport();
                var dateQuery = $"{_fromDate:dd/MM/yyyy} đến {_toDate:dd/MM/yyyy}";
                byte[] bytes;
                string fileName;

                if (_tabIndex == 0)
                {
                    if (_logs == null || !_logs.Any())
                    {
                        _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất Excel.");
                        return;
                    }
                    bytes = await export.GenerateTemperatureAlarmExcelAsync(_logs.ToList(), dateQuery);
                    fileName = $"TemperatureAlarmReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }
                else
                {
                    if (_dataLogs == null || !_dataLogs.Any())
                    {
                        _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất Excel.");
                        return;
                    }
                    bytes = await export.GenerateTemperatureDataLogExcelAsync(_dataLogs.ToList(), dateQuery);
                    fileName = $"TemperatureDataLogReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }

                await _js.InvokeVoidAsync("BlazorDownloadFile", fileName, bytes);
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi Xuất Excel", ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
