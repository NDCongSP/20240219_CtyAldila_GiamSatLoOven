using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Bản ghi cấu hình Part từ external DB (máy đo Auto Fre).
    /// Number = Item Number, khớp với FreMeasurementRecord.Part / FT14_TipOdFreq.PartName.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/Entities/Part.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-06-17
    /// </remarks>
    [Table("Part")]
    public class Part
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>Item Number (nvarchar(18), not null) — key khớp Part/PartName</summary>
        public string Number { get; set; } = string.Empty;

        public float? Flex_LL { get; set; }
        public float? Flex_UL { get; set; }
        public float? SW_LL { get; set; }
        public float? SW_UL { get; set; }
        public float? SW_Wt_LL { get; set; }
        public float? SW_Wt_UL { get; set; }
        public string? SW_Meas_Type { get; set; }
        public float? Freq_LL { get; set; }
        public float? Freq_UL { get; set; }
        public int? Freq_Std { get; set; }
        public float? Freq_BSL { get; set; }
        public short? Freq_Wt { get; set; }
    }
}
