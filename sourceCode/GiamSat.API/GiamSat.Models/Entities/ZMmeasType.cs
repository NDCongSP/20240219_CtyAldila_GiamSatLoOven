using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Loại điểm đo đường kính (ZM measurement type) từ external DB.
    /// ID khớp với PartZM.ZMID.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/Entities/ZMmeasType.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-06-17
    /// </remarks>
    [Table("ZMmeasType")]
    public class ZMmeasType
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>Tên loại điểm đo (nvarchar(25), not null)</summary>
        public string Name { get; set; } = string.Empty;
    }
}
