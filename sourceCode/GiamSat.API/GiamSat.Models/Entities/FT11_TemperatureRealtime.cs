using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Mỗi máy là 1 line trong bảng này, lưu dữ liệu realtime từ hệ thống giám sát nhiệt độ nhà máy.
    /// </summary>
    [Table("FT11")]
    public class FT11_TemperatureRealtime
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// List<TemperatureRealtimeModel>().
        /// </summary>
        [Column("C001")]
        public string?  C001_Data { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
