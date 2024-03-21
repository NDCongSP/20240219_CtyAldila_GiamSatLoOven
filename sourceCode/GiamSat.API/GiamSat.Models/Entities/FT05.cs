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
    /// Bảng lưu thông tin alarms.
    /// </summary>
    [Table("FT05")]
    public class FT05
    {
        [Key]
        public Guid Id { get; set; }
        public int OvenId { get; set; }
        public string OvenName { get; set; }
        public string? Description { get; set; }
        public int ACK { get; set; } = 0;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? ACKDate { get; set; } = DateTime.Now;
        public int Actived { get; set; } = 1;
    }
}
