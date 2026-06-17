using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Bản ghi cấu hình điểm đo đường kính (ZM) theo Part từ external DB.
    /// PartID khớp với Part.Id; ZMID là số thứ tự điểm đo.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/Entities/PartZM.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-06-17
    /// Note: Cấu hình keyless (HasNoKey) trong FreMeasurementDbContext — bảng tham chiếu chỉ đọc.
    /// </remarks>
    [Table("PartZM")]
    public class PartZM
    {
        public int? PartID { get; set; }
        public int? ZMID { get; set; }
        public float? Diam_LL { get; set; }
        public float? Diam_UL { get; set; }
        public float? Min_Under { get; set; }
        public float? Max_Over { get; set; }
    }
}
