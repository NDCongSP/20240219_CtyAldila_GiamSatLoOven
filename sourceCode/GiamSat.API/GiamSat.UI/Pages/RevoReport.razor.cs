using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;
using System.Net.Http;
using System.Net.Http.Json;
using ApiClient = GiamSat.APIClient;

namespace GiamSat.UI.Pages
{
    public partial class RevoReport
    {
        // Keep IHttpClientFactory only for PDF export (needs byte[] response)
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; } = default!;

        private RadzenDataGrid<FT09_RevoDatalog> _dataGrid = default!;
        private DateTime _fromDate = DateTime.Now.AddDays(-7);
        private DateTime _toDate = DateTime.Now;
        private int? _selectedRevoId = null;
        private bool _isLoading = false;
        private bool _hasData = false;

        private List<RevoDropdownModel> _revoList = new();
        private List<FT09_RevoDatalog> _reportData = new();
        private bool _gridInitialized = false;
        private int _shaftCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadRevoList();
        }

        private async Task LoadRevoList()
        {
            try
            {
                // Load FT07 via NSwag client
                var ft07Result = await _fT07Client.GetAllAsync();
                if (ft07Result.Succeeded && ft07Result.Data != null && ft07Result.Data.Count > 0)
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

                // Use NSwag client for FT09 GetFilter
                var filterModel = new ApiClient.RevoFilterModel
                {
                    GetAll = !_selectedRevoId.HasValue,
                    RevoId = _selectedRevoId,
                    FromDate = _fromDate,
                    ToDate = _toDate
                };

                var result = await _fT09Client.GetFilterAsync(filterModel);
                if (result.Succeeded && result.Data != null)
                {
                    // Map from APIClient model to Domain model for grid binding
                    _reportData = result.Data
                        .Select(x => new FT09_RevoDatalog
                        {
                            Id = x.Id,
                            CreatedAt = x.CreatedAt,
                            CreatedMachine = x.CreatedMachine,
                            RevoId = x.RevoId,
                            RevoName = x.RevoName,
                            Work = x.Work,
                            Part = x.Part,
                            Rev = x.Rev,
                            ColorCode = x.ColorCode,
                            Mandrel = x.Mandrel,
                            MandrelStart = x.MandrelStart,
                            StepId = x.StepId,
                            StepName = x.StepName,
                            StartedAt = x.StartedAt,
                            EndedAt = x.EndedAt,
                            ShaftNum = x.ShaftNum,
                            TotalTime = x.TotalTime
                        })
                        .OrderBy(x => x.ShaftNum)
                        .ThenBy(x => x.RevoName)
                        .ThenBy(x => x.StepId)
                        .ToList();
                    _hasData = _reportData.Any();
                    _shaftCount = _reportData.Select(x => x.ShaftNum).Where(x => x.HasValue).Distinct().Count();
                    _gridInitialized = false; // Reset to apply grouping on next render
                }
                else
                {
                    _reportData = new List<FT09_RevoDatalog>();
                    _hasData = false;
                    if (result.Messages != null && result.Messages.Count > 0)
                    {
                        var errorMessage = string.Join(", ", result.Messages);
                        _notificationService.Notify(NotificationSeverity.Error, "Lỗi", errorMessage);
                    }
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
            _shaftCount = 0;
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

                // Keep raw HttpClient for PDF export (NSwag returns void, we need byte[])
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

        private void OnGridRender(DataGridRenderEventArgs<FT09_RevoDatalog> args)
        {
            if (!_gridInitialized && _hasData && args.Grid != null)
            {
                // Apply group by ShaftNum
                args.Grid.Groups.Clear();
                args.Grid.Groups.Add(new GroupDescriptor
                {
                    Property = "ShaftNum",
                    Title = "Shaft",
                    SortOrder = SortOrder.Ascending
                });
                _gridInitialized = true;
            }
        }

        private void OnGroupRowRender(GroupRowRenderEventArgs args)
        {
            // Expand all groups by default
            args.Expanded = true;
        }

        public class RevoDropdownModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
