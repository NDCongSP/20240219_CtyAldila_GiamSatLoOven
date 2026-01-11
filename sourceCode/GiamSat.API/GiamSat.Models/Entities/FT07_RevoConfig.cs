using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("FT07")]
    public class FT07_RevoConfig
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Lưu thông tin cấu hình của tất cả các lò.
        /// RevoConfigs model.
        /// </summary>
        public string? C000 { get; set; }

        public bool? Actived { get; set; }=true;

        public DateTime? CreatedAt { get; set; }
    }
}
