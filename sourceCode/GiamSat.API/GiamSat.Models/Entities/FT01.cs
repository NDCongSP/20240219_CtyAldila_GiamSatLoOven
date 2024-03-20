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
    /// Bảng chứa các thông tin cấu hình về lò của hệ thống, số lượng lò và tên lò.
    /// </summary>
    [Table("FT01")]
    public class FT01
    {
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// List chứa thông tin cài đặt của tất cả các lò.
        /// Chính là ConfigModel.
        /// </summary>
        public string? C000 { get; set; }
        /// <summary>
        /// Chứa các thông tin config cho hệ thống.
        /// chính là list OvensInfo.
        /// </summary>
        public string? C001 { get; set; }
        public int Actived { get; set; } = 1;
        public DateTime? CreatedDate { get; set; }
    }
}
