using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using GiamSat.APIClient;
using System.Threading.Tasks;
using System.Linq;

namespace GiamSat.UI.Pages
{
    public partial class TemperatureConfig
    {
        [Inject] private ITemperatureConfigClient _temperatureConfigClient { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;

        private RadzenDataGrid<TemperatureConfigsModel> _dataGrid = default!;
        private List<TemperatureConfigsModel> _configs = new List<TemperatureConfigsModel>();
        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            _isLoading = true;
            StateHasChanged();
            try
            {
                var result = await _temperatureConfigClient.GetConfigsAsync();
                if (result != null)
                {
                    _configs = result.ToList();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Không thể tải cấu hình: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private async Task SaveData()
        {
            try
            {
                await _temperatureConfigClient.SaveConfigsAsync(_configs);
                NotificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã lưu cấu hình thành công");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        private async Task InsertRow()
        {
            var conf = new TemperatureConfigsModel {
                Id = _configs.Count > 0 ? _configs.Max(x => x.Id ?? 0) + 1 : 1,
                Name = $"Lò mới {_configs.Count + 1}",
                Path = "Local Station/Channel_Revo/Device1",
                Offset = 0,
                HightLevel = 45,
                LowLevel = -30,
                TimeBlinkAlarm = 1000
            };
            
            await _dataGrid.InsertRow(conf);
        }

        private async Task OnCreateRow(TemperatureConfigsModel config)
        {
            _configs.Add(config);
            await SaveData();
            await _dataGrid.Reload();
        }

        private async Task EditRow(TemperatureConfigsModel config)
        {
            await _dataGrid.EditRow(config);
        }

        private async Task OnUpdateRow(TemperatureConfigsModel config)
        {
            await SaveData();
        }

        private void CancelEdit(TemperatureConfigsModel config)
        {
            _dataGrid.CancelEditRow(config);
        }

        private async Task DeleteRow(TemperatureConfigsModel config)
        {
            if (_configs.Contains(config))
            {
                var confirm = await DialogService.Confirm($"Bạn có chắc muốn xóa {config.Name}?", "Xác nhận xóa",
                    new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });

                if (confirm == true)
                {
                    _configs.Remove(config);
                    await SaveData();
                    await _dataGrid.Reload();
                }
            }
        }
    }
}
