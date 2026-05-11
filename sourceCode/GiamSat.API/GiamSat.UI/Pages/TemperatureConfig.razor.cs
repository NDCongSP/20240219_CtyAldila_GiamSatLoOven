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

        private RadzenDataGrid<TemperatureLocationModel> _dataGrid = default!;
        private TemperatureConfigsModel _config = new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() };
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
                    _config = result;
                    if (_config.LocationsConfig == null)
                    {
                        _config.LocationsConfig = new List<TemperatureLocationModel>();
                    }
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
                // Notify winform to reload
                foreach (var loc in _config.LocationsConfig)
                {
                    loc.TriggerUpdate = true;
                }

                await _temperatureConfigClient.SaveConfigsAsync(_config);
                NotificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã lưu cấu hình thành công");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        private async Task SaveGlobalConfig()
        {
            await SaveData();
        }

        private async Task InsertRow()
        {
            var conf = new TemperatureLocationModel {
                Id = _config.LocationsConfig.Count > 0 ? _config.LocationsConfig.Max(x => x.Id ?? 0) + 1 : 1,
                Name = $"Lò mới {_config.LocationsConfig.Count + 1}",
                Path = "Local Station/ChannelTemperature1/Device1",
                Offset = 0,
                HightLevel = 45,
                LowLevel = -30
            };
            
            await _dataGrid.InsertRow(conf);
        }

        private async Task OnCreateRow(TemperatureLocationModel config)
        {
            _config.LocationsConfig.Add(config);
            await SaveData();
            await _dataGrid.Reload();
        }

        private async Task EditRow(TemperatureLocationModel config)
        {
            await _dataGrid.EditRow(config);
        }

        private async Task OnUpdateRow(TemperatureLocationModel config)
        {
            await SaveData();
        }

        private void CancelEdit(TemperatureLocationModel config)
        {
            _dataGrid.CancelEditRow(config);
        }

        private async Task DeleteRow(TemperatureLocationModel config)
        {
            if (_config.LocationsConfig.Contains(config))
            {
                var confirm = await DialogService.Confirm($"Bạn có chắc muốn xóa {config.Name}?", "Xác nhận xóa",
                    new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });

                if (confirm == true)
                {
                    _config.LocationsConfig.Remove(config);
                    await SaveData();
                    await _dataGrid.Reload();
                }
            }
        }
    }
}
