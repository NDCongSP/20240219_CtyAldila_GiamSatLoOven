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
        #region Các thông số của mẻ nung
        public int OvenId { get; set; }
        public string OvenName { get; set; }
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public int StepId { get; set; }
        public string StepName { get; set; }
        public double Setpoint { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        #endregion
        public double Temperature { get; set; }
        public string? Description { get; set; }
        public int ACK { get; set; } = 0;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? ACKDate { get; set; } = DateTime.Now;
        public int Actived { get; set; } = 1;

        /// <summary>
        /// RealtimeDisplayModel.
        /// </summary>
        public string Details { get; set; }
    }
}
