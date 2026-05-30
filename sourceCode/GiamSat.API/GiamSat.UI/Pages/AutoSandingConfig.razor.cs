using GiamSat.APIClient;
using GiamSat.Models;
using GiamSat.UI.Components;
using Microsoft.AspNetCore.Components;
using Radzen;
using FT14_TipOdFreq      = GiamSat.APIClient.FT14_TipOdFreq;
using AutoSandingTestRow  = GiamSat.Models.AutoSandingTestRow;

namespace GiamSat.UI.Pages
{
    public partial class AutoSandingConfig
    {
        // ── State ────────────────────────────────────────────────────────────
        private int  _selectedTab    = 0;
        private bool _isLoading      = true;
        private bool _isSaving       = false;
        private bool _isLoadingCalc  = false;
        private bool _abcdCalculated = false;
        private bool _showGuide      = false;

        // Tab 1 – danh sách part từ DB
        private List<FT14_TipOdFreq> _parts = new();

        // Tab 2 – tính ABCD
        private System.Guid? _selectedPartId = null;
        private List<AutoSandingTestRow> _testRows = new();
        private LinearRegressionResult _resultAB = new();
        private LinearRegressionResult _resultCD = new();

        // Tab 2 – form load data từ external DB
        private string  _work         = string.Empty;
        private double  _offsetFre1   = 0;
        private double  _offsetFre2   = 0;
        private double  _offsetSpine  = 0;
        private int     _formular     = 1;
        private double  _motorFrom    = 100;
        private double  _motorTo      = 500;
        private double  _motorStep    = 100;

        // Chart data
        private List<ChartPoint> _chartABPoints = new();
        private List<ChartPoint> _trendABPoints = new();
        private List<ChartPoint> _chartCDPoints = new();
        private List<ChartPoint> _trendCDPoints = new();

        // ── Lifecycle ────────────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // ── Tải dữ liệu từ DB ────────────────────────────────────────────────
        private async Task LoadData()
        {
            try
            {
                _isLoading = true;
                StateHasChanged();

                var result = await _fT14Client.GetAllAsync();
                if (result.Succeeded && result.Data != null)
                    _parts = result.Data.Where(x => x.Actived != false).OrderBy(x => x.PartName).ToList();
                else
                {
                    _parts = new List<FT14_TipOdFreq>();
                    var msg = result?.Messages != null ? string.Join(", ", result.Messages) : "API trả về lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi tải dữ liệu FT14", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", $"Không tải được dữ liệu: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        // ── Tab 1 – CRUD Part ────────────────────────────────────────────────
        private async Task OnAddPart()
        {
            var blank = new FT14_TipOdFreq
            {
                Id        = System.Guid.Empty,
                Actived   = true,
                Formula_F = 1,
            };

            var saved = await OpenPartDialog("Thêm Part mới", blank, isEdit: false);
            if (saved == null) return;

            try
            {
                saved.Id        = System.Guid.NewGuid();
                saved.CreatedAt = DateTime.Now;
                saved.CreatedMachine = Environment.MachineName;
                saved.Actived   = true;

                var result = await _fT14Client.InsertAsync(saved);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Thêm thành công", $"Đã thêm part \"{saved.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
        }

        private async Task OnEditPart(FT14_TipOdFreq item)
        {
            var saved = await OpenPartDialog($"Chỉnh sửa – {item.PartName}", item, isEdit: true);
            if (saved == null) return;

            try
            {
                saved.UpdateddAt = DateTime.Now;
                var result = await _fT14Client.UpdateAsync(saved);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Cập nhật thành công", $"Đã lưu part \"{saved.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
        }

        private async Task OnDeletePart(FT14_TipOdFreq item)
        {
            var confirm = await _dialogService.Confirm(
                $"Bạn có chắc muốn xóa part \"{item.PartName}\"?",
                "Xác nhận xóa",
                new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });

            if (confirm != true) return;

            try
            {
                // Soft-delete: set Actived = false
                item.Actived    = false;
                item.UpdateddAt = DateTime.Now;
                var result = await _fT14Client.UpdateAsync(item);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Đã xóa", $"Đã xóa part \"{item.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
        }

        private async Task<FT14_TipOdFreq?> OpenPartDialog(string title, FT14_TipOdFreq model, bool isEdit)
        {
            var result = await _dialogService.OpenAsync<DialogAutoSandingConfig>(
                title,
                new Dictionary<string, object>
                {
                    { "Model",  model  },
                    { "IsEdit", isEdit }
                },
                new DialogOptions { Width = "720px", Resizable = true, Draggable = true });

            return result as FT14_TipOdFreq;
        }

        // ── Tab 2 – Tính ABCD ────────────────────────────────────────────────
        private void OnPartSelected(object value)
        {
            _abcdCalculated = false;
            _testRows       = new List<AutoSandingTestRow>();
            StateHasChanged();
        }

        private void OnAddTestRow()
        {
            _testRows.Add(new AutoSandingTestRow { RowIndex = _testRows.Count + 1 });
            StateHasChanged();
        }

        private void OnRemoveLastRow()
        {
            if (_testRows.Count > 0) { _testRows.RemoveAt(_testRows.Count - 1); StateHasChanged(); }
        }

        private void OnLoadFakeData()
        {
            _testRows = new List<AutoSandingTestRow>
            {
                new() { RowIndex=1,  Fre1=308.9, BeltRotationRpm=100, Fre2=308.1, StiffnessY=2.546 },
                new() { RowIndex=2,  Fre1=309.4, BeltRotationRpm=100, Fre2=308.6, StiffnessY=2.552 },
                new() { RowIndex=3,  Fre1=309.3, BeltRotationRpm=200, Fre2=306.3, StiffnessY=2.526 },
                new() { RowIndex=4,  Fre1=309.5, BeltRotationRpm=200, Fre2=306.8, StiffnessY=2.526 },
                new() { RowIndex=5,  Fre1=308.3, BeltRotationRpm=300, Fre2=303.2, StiffnessY=2.436 },
                new() { RowIndex=6,  Fre1=309.1, BeltRotationRpm=300, Fre2=302.9, StiffnessY=2.445 },
                new() { RowIndex=7,  Fre1=306.6, BeltRotationRpm=400, Fre2=300.0, StiffnessY=2.398 },
                new() { RowIndex=8,  Fre1=307.4, BeltRotationRpm=400, Fre2=299.4, StiffnessY=2.366 },
                new() { RowIndex=9,  Fre1=307.9, BeltRotationRpm=500, Fre2=298.3, StiffnessY=2.372 },
                new() { RowIndex=10, Fre1=307.8, BeltRotationRpm=500, Fre2=298.7, StiffnessY=2.364 },
            };
            _abcdCalculated = false;
            _notificationService.Notify(NotificationSeverity.Info, "Fake Data", "Đã nạp 10 dòng dữ liệu mẫu từ slide 14.");
            StateHasChanged();
        }

        private async Task OnLoadDataFromDB()
        {
            var partName = _parts.FirstOrDefault(p => p.Id == _selectedPartId)?.PartName;
            if (string.IsNullOrWhiteSpace(partName))
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Chưa chọn Part", "Vui lòng chọn Part trước khi load data.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_work))
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Thiếu Work Order", "Vui lòng nhập Work Order.");
                return;
            }

            try
            {
                _isLoadingCalc = true;
                _abcdCalculated = false;
                StateHasChanged();

                var result = await _ft14CalcDataClient.GetCalcDataAsync(
                    partName, _work,
                    _offsetFre1, _offsetFre2,
                    _motorFrom, _motorTo, _motorStep);

                if (!result.Succeeded || result.Data == null)
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định từ API";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi load data", msg);
                    return;
                }

                // Map từ APIClient DTO → Models để dùng computed FreqDiff
                _testRows = result.Data.Select((r, idx) => new AutoSandingTestRow
                {
                    RowIndex        = idx + 1,
                    Fre1            = r.Fre1,
                    BeltRotationRpm = r.BeltRotationRpm,
                    Fre2            = r.Fre2,
                    StiffnessY      = r.StiffnessY,
                }).ToList();

                _notificationService.Notify(NotificationSeverity.Success, "Load thành công",
                    $"Đã load {_testRows.Count} dòng từ DB (Part={partName}, Work={_work}). Kiểm tra StiffnessY rồi nhấn Tính A,B,C,D.");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
            finally
            {
                _isLoadingCalc = false;
                StateHasChanged();
            }
        }

        private void OnCalculateABCD()
        {
            if (_testRows.Count < 2)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Thiếu dữ liệu", "Cần ít nhất 2 dòng để tính hồi quy.");
                return;
            }

            _resultAB = LinearRegression(_testRows.Select(r => r.StiffnessY).ToList(), _testRows.Select(r => r.Fre2).ToList());
            _resultCD = LinearRegression(_testRows.Select(r => r.FreqDiff).ToList(),   _testRows.Select(r => r.BeltRotationRpm).ToList());

            _chartABPoints = _testRows.Select(r => new ChartPoint { X = r.StiffnessY, Y = r.Fre2 }).ToList();
            _chartCDPoints = _testRows.Select(r => new ChartPoint { X = r.FreqDiff,   Y = r.BeltRotationRpm }).ToList();
            BuildTrendLine(_chartABPoints, _resultAB, out _trendABPoints);
            BuildTrendLine(_chartCDPoints, _resultCD, out _trendCDPoints);

            _abcdCalculated = true;
            StateHasChanged();
        }

        private async Task OnApplyAbcdToPart()
        {
            var part = _parts.FirstOrDefault(p => p.Id == _selectedPartId);
            if (part == null)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Chưa chọn Part", "Vui lòng chọn part trước khi áp dụng.");
                return;
            }

            try
            {
                _isSaving    = true;
                StateHasChanged();

                part.A           = Math.Round(_resultAB.Slope,     3);
                part.B           = Math.Round(_resultAB.Intercept,  3);
                part.C           = Math.Round(_resultCD.Slope,     3);
                part.D           = Math.Round(_resultCD.Intercept,  3);
                part.UpdateddAt  = DateTime.Now;

                var result = await _fT14Client.UpdateAsync(part);
                if (result.Succeeded)
                {
                    _notificationService.Notify(NotificationSeverity.Success, "Đã lưu DB",
                        $"A={part.A}, B={part.B}, C={part.C}, D={part.D} → part \"{part.PartName}\".");
                    await LoadData();
                }
                else
                {
                    var msg = result.Messages != null ? string.Join(", ", result.Messages) : "Lỗi không xác định";
                    _notificationService.Notify(NotificationSeverity.Error, "Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "Lỗi", ex.Message);
            }
            finally
            {
                _isSaving = false;
                StateHasChanged();
            }
        }

        // ── Toán học ─────────────────────────────────────────────────────────
        private static LinearRegressionResult LinearRegression(List<double> x, List<double> y)
        {
            int n = x.Count;
            double sumX  = x.Sum();
            double sumY  = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);
            double denom = n * sumX2 - sumX * sumX;
            double slope     = denom == 0 ? 0 : (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;
            double yMean = sumY / n;
            double ssTot = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssRes = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
            return new LinearRegressionResult { Slope = slope, Intercept = intercept, R2 = ssTot == 0 ? 1 : 1 - ssRes / ssTot };
        }

        private static void BuildTrendLine(List<ChartPoint> scatter, LinearRegressionResult reg, out List<ChartPoint> trend)
        {
            double xMin = scatter.Min(p => p.X), xMax = scatter.Max(p => p.X);
            trend = new List<ChartPoint>
            {
                new() { X = xMin, Y = reg.Slope * xMin + reg.Intercept },
                new() { X = xMax, Y = reg.Slope * xMax + reg.Intercept }
            };
        }

        public class ChartPoint { public double X { get; set; } public double Y { get; set; } }
    }
}
