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
    /// Mỗi máy là 1 line trong bảng này, lưu dữ liệu realtime từ Revo.
    /// </summary>
    [Table("FT08")]
    public class FT08_RevoRealtime
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// RevoConfigModel.Id
        /// </summary>
        [Column("C000")]
        public int? C000_RevoId { get; set; }

        /// <summary>
        /// RevoRealtimeModel
        /// </summary>
        [Column("C001")]
        public string?  C001_Data { get; set; }
    }
}
