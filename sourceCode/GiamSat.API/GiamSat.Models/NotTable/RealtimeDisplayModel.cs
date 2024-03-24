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
        /// 0-Dừng; 1- Chạy.
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// trạng thái kết nối modbus đến lò.
        /// 1-Kết nối; 0 - mất kết nối.
        /// </summary>
        public int ConnectionStatus { get; set; }
        /// <summary>
        /// Cảnh báo. 1 là cảnh báo, 0 là bình thường.
        /// </summary>
        public int Alarm { get; set; }
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

        /// <summary>
        /// Ngưỡng cao nhiệt độ. Dùng để cảnh báo.
        /// </summary>
        public double TemperatureHighLevel { get; set; }
    }
}
