using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Bản ghi đo tần số từ external DB (máy đo Auto Fre).
    /// Station = "Auto Fre No.1" → dữ liệu Fre1.
    /// Station = "Auto Fre No.2" → dữ liệu Fre2.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/Entities/FreMeasurementRecord.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-05-28
    /// Modified: 2026-05-31 — khớp schema thực tế DB: ShaftNum nullable, Reading/BSL/UL/LL là nvarchar, thêm IsCalib
    /// </remarks>
    [Table("DatalogFrequency")]
    public class FreMeasurementRecord
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Trạm đo — "Auto Fre No.1" = sand 1 / "Auto Fre No.2" = sand 2</summary>
        public string? Station { get; set; }

        /// <summary>Số thứ tự shaft trong batch (nullable theo DB)</summary>
        public int? ShaftNum { get; set; }

        public DateTime? DateTime { get; set; }

        /// <summary>Item Number — key khớp với FT14_TipOdFreq.PartName</summary>
        public string? Part { get; set; }

        /// <summary>Work Order — key tìm kiếm cùng với Part</summary>
        public string? WorkOrder { get; set; }

        public string? Standard { get; set; }

        public string? BSL { get; set; }

        public string? Weight { get; set; }

        /// <summary>Giá trị đo tần số (lưu dạng nvarchar trong DB) — parse sang double khi dùng</summary>
        public string? Reading { get; set; }

        public string? UL { get; set; }

        public string? LL { get; set; }

        public string? PassFail { get; set; }
        public string? BuildType { get; set; }
        public string? ShaftType { get; set; }

        public int? IsCalib { get; set; }
    }
}
