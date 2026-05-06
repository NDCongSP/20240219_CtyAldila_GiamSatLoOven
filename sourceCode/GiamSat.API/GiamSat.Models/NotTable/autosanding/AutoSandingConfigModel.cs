using System;
using System.Collections.Generic;

namespace GiamSat.Models
{
    public class AutoSandingConfigs : List<AutoSandingConfigModel> { }

    /// <summary>
    /// Cấu hình chung cho từng part trên line Auto Sanding.
    /// </summary>
    public class AutoSandingConfigModel
    {
        public int? Id { get; set; }

        /// <summary>Tên / mã part (Item Number)</summary>
        public string? ItemNumber { get; set; }

        /// <summary>Chiều dài shaft (mm)</summary>
        public double Length { get; set; }

        /// <summary>Tần số mục tiêu (CPM)</summary>
        public double FreqTarget { get; set; }

        /// <summary>Dung sai âm</summary>
        public double ToleranceLow { get; set; }

        /// <summary>Dung sai dương</summary>
        public double ToleranceHigh { get; set; }

        /// <summary>Đường kính dưới (mm)</summary>
        public double DiamLL { get; set; }

        /// <summary>Đường kính trên (mm)</summary>
        public double DiamUL { get; set; }

        /// <summary>Tip OD Length type (vd: UPT TOD @76MM)</summary>
        public string? TipOdLength { get; set; }

        /// <summary>Công thức mòn belt (F index)</summary>
        public int FormulaF { get; set; } = 1;

        // ── Tham số tính toán (kết quả từ tab "Tính ABCD") ──────────────
        /// <summary>Hệ số A: Y = A*Z + B (Stiffness vs Frequency)</summary>
        public double A { get; set; }

        /// <summary>Hệ số B: Y = A*Z + B</summary>
        public double B { get; set; }

        /// <summary>Hệ số C: R = C*S + D (Belt rotation vs Freq diff)</summary>
        public double C { get; set; }

        /// <summary>Hệ số D: R = C*S + D</summary>
        public double D { get; set; }

        /// <summary>Ghi chú</summary>
        public string? Note { get; set; }
    }

    /// <summary>
    /// Một dòng dữ liệu test dùng để tính A,B,C,D cho 1 part.
    /// </summary>
    public class AutoSandingTestRow
    {
        public int RowIndex { get; set; }

        // -- Sand lần 1 -------------------------------------------------------
        /// <summary>Fre1: Tần số đo sau sand lần 1 (CPM)</summary>
        public double Fre1 { get; set; }

        /// <summary>Z: Độ cứng đo sau sand lần 1 (Kg)</summary>
        public double StiffnessZ { get; set; }

        // -- Sand lần 2 -------------------------------------------------------
        /// <summary>Tốc độ motor cố định khi sand lần 2 (RPM)</summary>
        public double BeltRotationRpm { get; set; }

        /// <summary>Fre2: Tần số đo sau sand lần 2 (CPM)</summary>
        public double Fre2 { get; set; }

        /// <summary>Y: Độ cứng đo sau sand lần 2 (Kg)</summary>
        public double StiffnessY { get; set; }

        // -- Tính tự động -------------------------------------------------------
        /// <summary>S = Fre1 – Fre2</summary>
        public double FreqDiff => Math.Round(Fre1 - Fre2, 3);
    }

    /// <summary>
    /// Kết quả hồi quy tuyến tính (A,B) hoặc (C,D).
    /// </summary>
    public class LinearRegressionResult
    {
        public double Slope { get; set; }       // A hoặc C
        public double Intercept { get; set; }   // B hoặc D
        public double R2 { get; set; }          // Hệ số xác định (độ chính xác)
    }
}
