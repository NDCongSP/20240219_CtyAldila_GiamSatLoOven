using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GiamSat.UI.Pages
{
    public partial class RevoReport
    {
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;

        private RadzenDataGrid<FT09_RevoDatalog> _dataGrid = default!;
        private DateTime _fromDate = DateTime.Now.AddDays(-7);
        private DateTime _toDate = DateTime.Now;
        private int? _selectedRevoId = null;
        private bool _isLoading = false;
        private bool _hasData = false;

        private List<RevoDropdownModel> _revoList = new();
        private List<FT09_RevoDatalog> _reportData = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadRevoList();
        }

        private async Task LoadRevoList()
        {
            try
            {
                var httpClient = HttpClientFactory.CreateClient("GiamSatAPI");
                var ft07Response = await httpClient.GetAsync("api/FT07");
                if (ft07Response.IsSuccessStatusCode)
                {
                    var ft07Result = await ft07Response.Content.ReadFromJsonAsync<Result<List<FT07_RevoConfig>>>();
                    if (ft07Result != null && ft07Result.Succeeded && ft07Result.Data != null && ft07Result.Data.Count > 0)
                    {
                        var ft07 = ft07Result.Data.FirstOrDefault(x => x.Actived == true) ?? ft07Result.Data.FirstOrDefault();
                        if (ft07 != null && !string.IsNullOrEmpty(ft07.C000))
                        {
                            var revoConfigs = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);
                            if (revoConfigs != null)
                            {
                                _revoList = revoConfigs.Select(x => new RevoDropdownModel
                                {
                                    Id = x.Id ?? 0,
                                    Name = x.Name ?? $"REVO {x.Id}"
                                }).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", $"Không thể tải danh sách REVO: {ex.Message}");
            }
        }

        private async Task OnSearch()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                var httpClient = HttpClientFactory.CreateClient("GiamSatAPI");
                var filterModel = new RevoFilterModel
                {
                    GetAll = !_selectedRevoId.HasValue,
                    RevoId = _selectedRevoId,
                    FromDate = _fromDate,
                    ToDate = _toDate
                };

                var response = await httpClient.PostAsJsonAsync("api/FT09/GetFilter", filterModel);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Result<List<FT09_RevoDatalog>>>();
                    if (result != null && result.Succeeded && result.Data != null)
                    {
                        _reportData = result.Data.OrderBy(x => x.StartedAt ?? x.CreatedAt ?? DateTime.MinValue).ToList();
                        _hasData = _reportData.Any();
                    }
                    else
                    {
                        _reportData = new List<FT09_RevoDatalog>();
                        _hasData = false;
                        if (result != null && result.Messages != null && result.Messages.Count > 0)
                        {
                            var errorMessage = string.Join(", ", result.Messages);
                            _notificationService.Notify(NotificationSeverity.Error, "Lỗi", errorMessage);
                        }
                    }
                }
                else
                {
                    _reportData = new List<FT09_RevoDatalog>();
                    _hasData = false;
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", "Không thể tải dữ liệu từ server");
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi tải dữ liệu: {ex.Message}");
                _hasData = false;
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private void OnReset()
        {
            _fromDate = DateTime.Now.AddDays(-7);
            _toDate = DateTime.Now;
            _selectedRevoId = null;
            _hasData = false;
            _reportData.Clear();
            StateHasChanged();
        }

        private async Task OnExportExcel()
        {
            try
            {
                if (!_hasData || _reportData.Count == 0)
                {
                    _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất");
                    return;
                }

                var excel = new ExcelExportRevo();
                var excelBytes = excel.GenerateExcelFileAsync(_reportData, $"{_fromDate:dd/MM/yyyy} đến {_toDate:dd/MM/yyyy}");
                var filename = $"BaoCao_REVO_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                await _js.InvokeVoidAsync("BlazorDownloadFile", filename, excelBytes);
                _notificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã xuất file Excel thành công");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi xuất Excel: {ex.Message}");
            }
        }

        private async Task OnExportPDF()
        {
            try
            {
                if (!_hasData || _reportData.Count == 0)
                {
                    _notificationService.Notify(NotificationSeverity.Warning, "Cảnh báo", "Không có dữ liệu để xuất");
                    return;
                }

                var httpClient = HttpClientFactory.CreateClient("GiamSatAPI");
                var filterModel = new RevoFilterModel
                {
                    GetAll = !_selectedRevoId.HasValue,
                    RevoId = _selectedRevoId,
                    FromDate = _fromDate,
                    ToDate = _toDate
                };

                var response = await httpClient.PostAsJsonAsync("api/FT09/ExportPdf", filterModel);
                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentDisposition = response.Content.Headers.ContentDisposition;
                    var filename = contentDisposition?.FileName ?? $"BaoCao_REVO_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    
                    // Remove quotes from filename if present
                    filename = filename.Trim('"');
                    
                    await _js.InvokeVoidAsync("BlazorDownloadFile", filename, pdfBytes);
                    _notificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã xuất file PDF thành công");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Không thể xuất PDF: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Lỗi khi xuất PDF: {ex.Message}");
            }
        }

        public class RevoDropdownModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
