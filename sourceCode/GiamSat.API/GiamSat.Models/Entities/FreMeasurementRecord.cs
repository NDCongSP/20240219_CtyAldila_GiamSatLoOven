using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Bản ghi đo tần số từ external DB (máy đo Auto Fre).
    /// Station = "Auto Fre No.1" → dữ liệu Fre1 (trước sand 2).
    /// Station = "Auto Fre No.2" → dữ liệu Fre2 (sau sand 2).
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/Entities/FreMeasurementRecord.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-05-28
    /// </remarks>
    [Table("DatalogFrequency")]
    public class FreMeasurementRecord
    {
        [Key]
        public long Id { get; set; }

        /// <summary>Trạm đo — "Auto Fre No.1" = sand 1 / "Auto Fre No.2" = sand 2</summary>
        public string? Station { get; set; }

        /// <summary>Số thứ tự shaft trong batch</summary>
        public int ShaftNum { get; set; }

        public DateTime? DateTime { get; set; }

        /// <summary>Item Number — key khớp với FT14_TipOdFreq.PartName</summary>
        public string? Part { get; set; }

        /// <summary>Work Order — key tìm kiếm cùng với Part</summary>
        public string? WorkOrder { get; set; }

        public string? Standard { get; set; }

        /// <summary>BSL (Baseline / Spec)</summary>
        public double? BSL { get; set; }

        public double? Weight { get; set; }

        /// <summary>Giá trị đo tần số (CPM) — Fre1 hoặc Fre2 tuỳ Station</summary>
        public double? Reading { get; set; }

        /// <summary>Upper Limit</summary>
        public double? UL { get; set; }

        /// <summary>Lower Limit</summary>
        public double? LL { get; set; }

        public string? PassFail { get; set; }
        public string? BuildType { get; set; }
        public string? ShaftType { get; set; }
    }
}
