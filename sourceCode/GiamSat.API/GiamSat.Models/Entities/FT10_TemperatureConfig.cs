using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Bang cấu hình cho các vị trị đo nhiệt độ.
    /// </summary>
    [Table("FT10")]
    public class FT10_TemperatureConfig
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Lưu thông tin cấu hình của tất cả các lò.
        /// List<TemperatureConfigsModel>().
        /// </summary>
        public string? C000 { get; set; }

        public bool? Actived { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
    }
}
