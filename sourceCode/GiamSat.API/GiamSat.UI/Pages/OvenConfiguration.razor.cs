using GiamSat.Models;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using ApiClient = GiamSat.APIClient;

namespace GiamSat.UI.Pages
{
    public partial class OvenConfiguration
    {
        [Parameter]
        public int OvenId { get; set; }

        private RadzenDataGrid<RevoConfigModel> _dataGrid = default!;
        private RevoConfigs _revoConfigs = new RevoConfigs();
        private bool _isLoading = true;
        private ApiClient.FT07_RevoConfig? _ft07Record = null;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected override async Task OnParametersSetAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                // Get all FT07 records via NSwag client
                var result = await _fT07Client.GetAllAsync();
                if (result.Succeeded && result.Data != null && result.Data.Count > 0)
                {
                    // Get the first active record or create new one
                    _ft07Record = result.Data.FirstOrDefault(x => x.Actived == true) ?? result.Data.FirstOrDefault();
                    
                    if (_ft07Record != null && !string.IsNullOrEmpty(_ft07Record.C000))
                    {
                        _revoConfigs = JsonConvert.DeserializeObject<RevoConfigs>(_ft07Record.C000) ?? new RevoConfigs();
                    }
                    else
                    {
                        _revoConfigs = new RevoConfigs();
                    }
                }
                else
                {
                    _revoConfigs = new RevoConfigs();
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi tải dữ liệu: {ex.Message}");
                _revoConfigs = new RevoConfigs();
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
                var jsonData = JsonConvert.SerializeObject(_revoConfigs);
                
                if (_ft07Record == null)
                {
                    // Create new record via NSwag client
                    var newRecord = new ApiClient.FT07_RevoConfig
                    {
                        Id = Guid.NewGuid(),
                        C000 = jsonData,
                        Actived = true,
                        CreatedAt = DateTime.Now
                    };

                    var result = await _fT07Client.InsertAsync(newRecord);
                    if (result.Succeeded)
                    {
                        _ft07Record = result.Data;
                    }
                    else
                    {
                        var errorMsg = result.Messages != null ? string.Join(", ", result.Messages) : "Không thể tạo mới";
                        _notificationService.Notify(NotificationSeverity.Error, "Lỗi", errorMsg);
                        return;
                    }
                }
                else
                {
                    // Update existing record via NSwag client
                    _ft07Record.C000 = jsonData;
                    var result = await _fT07Client.UpdateAsync(_ft07Record);
                    if (!result.Succeeded)
                    {
                        var errorMsg = result.Messages != null ? string.Join(", ", result.Messages) : "Không thể cập nhật";
                        _notificationService.Notify(NotificationSeverity.Error, "Lỗi", errorMsg);
                        return;
                    }
                }

                _notificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã lưu dữ liệu thành công");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        private async Task OnAddRevo()
        {
            var newRevo = new RevoConfigModel
            {
                Id = _revoConfigs.Count > 0 ? _revoConfigs.Max(x => x.Id ?? 0) + 1 : 1,
                Name = $"REVO {(_revoConfigs.Count + 1)}",
                Path = "",
                ConstringAccessDb = "",
                Pulse_Rev = 3200,
                SaveMode = EnumSaveMode.Save
            };

            var result = await _dialogService.OpenAsync<DialogRevoConfig>("Thêm REVO mới",
                new Dictionary<string, object> { { "Model", newRevo }, { "IsEdit", false } },
                new DialogOptions { Width = "500px", Resizable = true, Draggable = true });

            if (result is RevoConfigModel savedRevo)
            {
                _revoConfigs.Add(savedRevo);
                await SaveData();
                await _dataGrid.Reload();
            }
        }

        private async Task OnEdit(RevoConfigModel item)
        {
            var editItem = new RevoConfigModel
            {
                Id = item.Id,
                Name = item.Name,
                Path = item.Path,
                ConstringAccessDb = item.ConstringAccessDb,
                Pulse_Rev = item.Pulse_Rev,
                IntervalResetShaft = item.IntervalResetShaft,
                SaveMode = item.SaveMode
            };

            var result = await _dialogService.OpenAsync<DialogRevoConfig>("Chỉnh sửa REVO",
                new Dictionary<string, object> { { "Model", editItem }, { "IsEdit", true } },
                new DialogOptions { Width = "500px", Resizable = true, Draggable = true });

            if (result is RevoConfigModel savedRevo)
            {
                var index = _revoConfigs.FindIndex(x => x.Id == item.Id);
                if (index >= 0)
                {
                    _revoConfigs[index] = savedRevo;
                    await SaveData();
                    await _dataGrid.Reload();
                }
            }
        }

        private async Task OnDelete(RevoConfigModel item)
        {
            var confirm = await _dialogService.Confirm($"Bạn có chắc muốn xóa {item.Name}?", "Xác nhận xóa",
                new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });

            if (confirm == true)
            {
                _revoConfigs.Remove(item);
                await SaveData();
                await _dataGrid.Reload();
                _notificationService.Notify(NotificationSeverity.Success, "Thành công", $"Đã xóa {item.Name}");
            }
        }
    }
}
