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
        [Inject] private NotificationService NotificationService { get; set; } = default!;

        private RadzenDataGrid<FT13_TemperatureAlarmLog> _dataGrid = default!;
        private ICollection<FT13_TemperatureAlarmLog> _logs = new List<FT13_TemperatureAlarmLog>();
        
        private DateTime _fromDate = DateTime.Today;
        private DateTime _toDate = DateTime.Today;
        private bool _isLoading = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
             _isLoading = true;
            try
            {
                var result = await _temperatureDataClient.GetAlarmLogsAsync(_fromDate, _toDate);
                _logs = result ?? new List<FT13_TemperatureAlarmLog>();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi Server", $"Không thể tải báo cáo: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task ExportExcel()
        {
            if (_logs == null || !_logs.Any())
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất Excel.");
                return;
            }

            try
            {
                _isLoading = true;
                var export = new ExcelExport();
                var dateQuery = $"{_fromDate:dd/MM/yyyy} đến {_toDate:dd/MM/yyyy}";
                var bytes = await export.GenerateTemperatureAlarmExcelAsync(_logs.ToList(), dateQuery);

                var fileName = $"TemperatureAlarmReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                await _js.InvokeVoidAsync("BlazorDownloadFile", fileName, bytes);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi Xuất Excel", ex.Message);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
