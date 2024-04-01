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
        /// <summary>
        /// Số thứ tự mỗi lần chạy.được tạo ra mỗi khi bắt đầu run profile, và kết thúc khi dùng profile.
        /// Và sẽ được tạo mới khi run profile mới.
        /// Mục đích phục vụ cho việc lọc data theo từng mẻ nung
        /// </summary>
        public Guid ZIndex { get; set; }

        /// <summary>
        /// Nhiệt độ thực tế
        /// </summary>
        public double Temperature { get; set; }
        /// <summary>
        /// Chính là BeginTime.
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// sẽ được log vào khi lò chuyển trạng thái từ đang run sang dùng profile.
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? EndTime { get; set; }
        public int Actived { get; set; } = 1;
        public string CreatedMachine { get; set; } = Environment.MachineName;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Trạng thái của lò.
        /// 1-CHẠY 0-DỪNG.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// RealtimeDisplayModel.
        /// </summary>
        public string Details { get; set; }
    }
}
