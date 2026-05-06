using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace GiamSat.UI.Pages
{
    public partial class AutoSandingConfig
    {
        // ── State ─────────────────────────────────────────────────────────────
        private int _selectedTab = 0;
        private bool _isLoading = true;
        private bool _isSaving = false;
        private bool _abcdCalculated = false;

        // Tab 1
        private AutoSandingConfigs _configs = new();
        private RadzenDataGrid<AutoSandingConfigModel> _configGrid = default!;

        // Tab 2
        private int? _selectedPartId = null;
        private List<AutoSandingTestRow> _testRows = new();
        private LinearRegressionResult _resultAB = new();
        private LinearRegressionResult _resultCD = new();

        // Chart data
        private List<ChartPoint> _chartABPoints = new();
        private List<ChartPoint> _trendABPoints = new();
        private List<ChartPoint> _chartCDPoints = new();
        private List<ChartPoint> _trendCDPoints = new();

        // ── Lifecycle ─────────────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // ── Tải dữ liệu ───────────────────────────────────────────────────────
        private async Task LoadData()
        {
            _isLoading = true;
            StateHasChanged();

            await Task.Delay(300); // TODO: thay bằng gọi API thực

            // Fake data mẫu (từ slide 16)
            _configs = new AutoSandingConfigs
            {
                new AutoSandingConfigModel
                {
                    Id = 1,
                    ItemNumber  = "AU228-IR-7",
                    Length      = 1169,
                    FreqTarget  = 298,
                    ToleranceLow  = 1.5,
                    ToleranceHigh = 3.5,
                    DiamLL      = 8.40,
                    DiamUL      = 8.56,
                    TipOdLength = "UPT TOD @76MM",
                    FormulaF    = 1,
                    A = 51.080, B = 177.920,
                    C = 45.278, D = 65.005
                },
                new AutoSandingConfigModel
                {
                    Id = 2,
                    ItemNumber  = "AU228-IR-7",
                    Length      = 1169,
                    FreqTarget  = 298,
                    ToleranceLow  = 1.5,
                    ToleranceHigh = 3.5,
                    DiamLL      = 8.40,
                    DiamUL      = 8.56,
                    TipOdLength = "UPT TOD @25MM",
                    FormulaF    = 1,
                    A = 51.080, B = 177.920,
                    C = 45.278, D = 65.005
                }
            };

            _isLoading = false;
            StateHasChanged();
        }

        // ── Tab 1 – Cấu hình chung ─────────────────────────────────────────
        private async Task OnAddPart()
        {
            var newPart = new AutoSandingConfigModel
            {
                Id = _configs.Count > 0 ? (_configs.Max(x => x.Id ?? 0) + 1) : 1,
                ItemNumber = "NEW-PART",
                Length = 0,
                FreqTarget = 0,
                FormulaF = 1
            };
            _configs.Add(newPart);
            await _configGrid.Reload();
            _notificationService.Notify(NotificationSeverity.Info, "Thêm mới",
                "Đã thêm dòng mới. Vui lòng chỉnh sửa thông tin.");
        }

        private async Task OnEditPart(AutoSandingConfigModel item)
        {
            // TODO: mở dialog chỉnh sửa chi tiết
            _notificationService.Notify(NotificationSeverity.Info, "Chỉnh sửa",
                $"Chức năng chỉnh sửa chi tiết part {item.ItemNumber} sẽ được bổ sung.");
            await Task.CompletedTask;
        }

        private async Task OnDeletePart(AutoSandingConfigModel item)
        {
            var confirm = await _dialogService.Confirm(
                $"Bạn có chắc muốn xóa part \"{item.ItemNumber}\"?",
                "Xác nhận xóa",
                new ConfirmOptions { OkButtonText = "Xóa", CancelButtonText = "Hủy" });

            if (confirm == true)
            {
                _configs.Remove(item);
                await _configGrid.Reload();
                _notificationService.Notify(NotificationSeverity.Success, "Đã xóa",
                    $"Đã xóa part {item.ItemNumber}");
            }
        }

        private async Task OnSaveConfig()
        {
            _isSaving = true;
            StateHasChanged();

            await Task.Delay(500); // TODO: thay bằng gọi API thực

            _isSaving = false;
            _notificationService.Notify(NotificationSeverity.Success, "Thành công",
                "Đã lưu cấu hình (fake – chưa kết nối DB).");
            StateHasChanged();
        }

        // ── Tab 2 – Tính ABCD ─────────────────────────────────────────────
        private void OnPartSelected(object value)
        {
            _abcdCalculated = false;
            _testRows = new List<AutoSandingTestRow>();
            StateHasChanged();
        }

        private void OnAddTestRow()
        {
            _testRows.Add(new AutoSandingTestRow
            {
                RowIndex = _testRows.Count + 1
            });
            StateHasChanged();
        }

        private void OnRemoveLastRow()
        {
            if (_testRows.Count > 0)
            {
                _testRows.RemoveAt(_testRows.Count - 1);
                StateHasChanged();
            }
        }

        /// <summary>Nạp dữ liệu mẫu từ slide 14 để test.</summary>
        private void OnLoadFakeData()
        {
            _testRows = new List<AutoSandingTestRow>
            {
                new() { RowIndex=1,  Fre1=308.9, StiffnessZ=2.559, BeltRotationRpm=100, Fre2=308.1, StiffnessY=2.546 },
                new() { RowIndex=2,  Fre1=309.4, StiffnessZ=2.566, BeltRotationRpm=100, Fre2=308.6, StiffnessY=2.552 },
                new() { RowIndex=3,  Fre1=309.3, StiffnessZ=2.580, BeltRotationRpm=200, Fre2=306.3, StiffnessY=2.526 },
                new() { RowIndex=4,  Fre1=309.5, StiffnessZ=2.574, BeltRotationRpm=200, Fre2=306.8, StiffnessY=2.526 },
                new() { RowIndex=5,  Fre1=308.3, StiffnessZ=2.529, BeltRotationRpm=300, Fre2=303.2, StiffnessY=2.436 },
                new() { RowIndex=6,  Fre1=309.1, StiffnessZ=2.527, BeltRotationRpm=300, Fre2=302.9, StiffnessY=2.445 },
                new() { RowIndex=7,  Fre1=306.6, StiffnessZ=2.529, BeltRotationRpm=400, Fre2=300.0, StiffnessY=2.398 },
                new() { RowIndex=8,  Fre1=307.4, StiffnessZ=2.521, BeltRotationRpm=400, Fre2=299.4, StiffnessY=2.366 },
                new() { RowIndex=9,  Fre1=307.9, StiffnessZ=2.531, BeltRotationRpm=500, Fre2=298.3, StiffnessY=2.372 },
                new() { RowIndex=10, Fre1=307.8, StiffnessZ=2.542, BeltRotationRpm=500, Fre2=298.7, StiffnessY=2.364 },
            };
            _abcdCalculated = false;
            _notificationService.Notify(NotificationSeverity.Info, "Fake Data",
                "Đã nạp 10 dòng dữ liệu mẫu từ slide 14.");
            StateHasChanged();
        }

        /// <summary>Tính A,B,C,D bằng hồi quy tuyến tính.</summary>
        private void OnCalculateABCD()
        {
            if (_testRows.Count < 2)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Thiếu dữ liệu",
                    "Cần ít nhất 2 dòng dữ liệu để tính hồi quy.");
                return;
            }

            // --- A, B: Y (Fre2 / CPM) = A * Z (StiffnessY / kg) + B ---
            var abX = _testRows.Select(r => r.StiffnessY).ToList();
            var abY = _testRows.Select(r => r.Fre2).ToList();
            _resultAB = LinearRegression(abX, abY);

            // --- C, D: R (RPM) = C * S (FreqDiff) + D ---
            var cdX = _testRows.Select(r => r.FreqDiff).ToList();
            var cdY = _testRows.Select(r => r.BeltRotationRpm).ToList();
            _resultCD = LinearRegression(cdX, cdY);

            // Chart scatter points
            _chartABPoints = _testRows
                .Select(r => new ChartPoint { X = r.StiffnessY, Y = r.Fre2 })
                .ToList();

            _chartCDPoints = _testRows
                .Select(r => new ChartPoint { X = r.FreqDiff, Y = r.BeltRotationRpm })
                .ToList();

            // Trend lines (2 điểm đầu–cuối)
            BuildTrendLine(_chartABPoints, _resultAB, out _trendABPoints);
            BuildTrendLine(_chartCDPoints, _resultCD, out _trendCDPoints);

            _abcdCalculated = true;
            StateHasChanged();
        }

        /// <summary>Áp dụng kết quả A,B,C,D vào part đang chọn.</summary>
        private async Task OnApplyAbcdToPart()
        {
            var part = _configs.FirstOrDefault(p => p.Id == _selectedPartId);
            if (part == null)
            {
                _notificationService.Notify(NotificationSeverity.Warning, "Chưa chọn part",
                    "Vui lòng chọn part trước khi áp dụng.");
                return;
            }

            part.A = Math.Round(_resultAB.Slope, 3);
            part.B = Math.Round(_resultAB.Intercept, 3);
            part.C = Math.Round(_resultCD.Slope, 3);
            part.D = Math.Round(_resultCD.Intercept, 3);

            await _configGrid.Reload();
            _notificationService.Notify(NotificationSeverity.Success, "Áp dụng thành công",
                $"Đã cập nhật A={part.A}, B={part.B}, C={part.C}, D={part.D} vào part {part.ItemNumber}.");
        }

        // ── Toán học ──────────────────────────────────────────────────────────

        /// <summary>Hồi quy tuyến tính đơn giản: y = slope*x + intercept</summary>
        private static LinearRegressionResult LinearRegression(
            List<double> x, List<double> y)
        {
            int n = x.Count;
            double sumX  = x.Sum();
            double sumY  = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            double denom = n * sumX2 - sumX * sumX;
            double slope     = denom == 0 ? 0 : (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;

            // R²
            double yMean = sumY / n;
            double ssTot = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssRes = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
            double r2 = ssTot == 0 ? 1 : 1 - ssRes / ssTot;

            return new LinearRegressionResult
            {
                Slope     = slope,
                Intercept = intercept,
                R2        = r2
            };
        }

        private static void BuildTrendLine(
            List<ChartPoint> scatter,
            LinearRegressionResult reg,
            out List<ChartPoint> trend)
        {
            double xMin = scatter.Min(p => p.X);
            double xMax = scatter.Max(p => p.X);
            trend = new List<ChartPoint>
            {
                new() { X = xMin, Y = reg.Slope * xMin + reg.Intercept },
                new() { X = xMax, Y = reg.Slope * xMax + reg.Intercept }
            };
        }

        // ── Helper ────────────────────────────────────────────────────────────
        public class ChartPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}
