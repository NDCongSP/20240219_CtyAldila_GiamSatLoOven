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
    /// Log data nhiệt độ của lò khi lò Run profile.
    /// </summary>
    [Table("FT04")]
    public class FT04
    {
        [Key]
        public Guid Id { get; set; }
        public int OvenId { get; set; }
        public string OvenName { get; set; }    
        public double Temperature { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// sẽ được log vào khi lò chuyển trạng thái từ đang run sang dùng profile.
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? EndTime { get; set; }
        public int Actived { get; set; } = 1;
        public string CreatedMachine { get; set; } = Environment.MachineName;
    }
}
