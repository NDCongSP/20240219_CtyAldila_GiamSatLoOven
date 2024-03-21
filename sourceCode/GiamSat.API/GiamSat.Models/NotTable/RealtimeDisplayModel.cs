using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Chứa thông tin cần hiển thị của 1 lò.
    /// </summary>
    public class RealtimeDisplayModel
    {
        public int OvenId { get; set; }
        public string OvenName { get; set; }
        /// <summary>
        /// Trạng thái lò Chạy/Dừng.
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// trạng thái kết nối modbus đến lò.
        /// </summary>
        public string ConectionStatus { get; set; }
        public double Temperature { get; set; } = 0;
        /// <summary>
        /// Trạng thái cửa.
        /// 1-đóng; 0- mở.
        /// </summary>
        public string Door { get; set; }
        /// <summary>
        /// Profile đang chạy.
        /// </summary>
        public int ProfileNumber_CurrentStatus { get; set; }
        public string ProfileName { get; set; }
        /// <summary>
        /// Bước đang chạy.
        /// </summary>
        public int ProfileStepNumber_CurrentStatus { get; set; }
        /// <summary>
        /// Kiểu bước chạy.
        /// RampTime,RampRate,Soak,Kump,End
        /// </summary>
        public EnumProfileStepType ProfileStepType_CurrentStatus { get; set; }
        public int HoursRemaining_CurrentStatus { get; set; }
        public int MinutesRemaining_CurrentStatus { get; set; }
        public int SecondsRemaining_CurrentStatus { get; set; }
    }
}
