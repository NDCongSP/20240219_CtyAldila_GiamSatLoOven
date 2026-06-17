using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Bản ghi thiết lập mới theo Part từ external DB (1-1 với Part qua PartId).
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/Entities/PartNewSetting.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-06-17
    /// </remarks>
    [Table("PartNewSetting")]
    public class PartNewSetting
    {
        /// <summary>FK tới Part.Id (not null) — dùng làm khóa chính 1-1</summary>
        [Key]
        public int PartId { get; set; }

        public float? LLI { get; set; }
        public float? LUI { get; set; }
        public float? LStd { get; set; }
        public float? FR { get; set; }
        public bool? TL { get; set; }
        public float? Freq_CP { get; set; }
        public float? Freq_PD { get; set; }
    }
}
