using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class ConfigModel
    {
        /// <summary>
        /// Thời gian này dùng để check nhiệt độ để cảnh báo trong suốt quá trình chạy lò (ms)..
        /// Sau khoảng thời gian này mà lò ko thay đổi nhiệt độ thì sẽ cảnh báo
        /// </summary>
        public int TimeTempChange { get; set; }
        /// <summary>
        /// Quy định số lẻ nhiệt độ, nhân với giá trị nhiệt độ đọc về để có số lẻ hay không.
        /// </summary>
        public double Gain { get; set; } = 1;
        /// <summary>
        /// Chu kỳ thời gian log data (ms).
        /// </summary>
        public int DataLogInterval { get; set; } = 5000;
        /// <summary>
        /// Chu kỳ thời gian log data khi chạy profile ((ms).
        /// </summary>
        public int DataLogWhenRunProfileInterval { get; set; } = 1000;
        /// <summary>
        /// Thời gian ghi giá trị hiển thị lên web (ms).
        /// </summary>
        public int DisplayRealtimeInterval { get; set; } = 1000;//chu kỳ update data hiển thị. đơn vị giây

        /// <summary>
        /// Thời gian để đếm nhận biết lò chạy hay dùng. Nếu giá trị Second trả về ko thay đổi sau khoảng thời gian này thì sẽ cảnh báo lò dừng.
        /// Đơn vị (ms).
        /// </summary>
        public int CountSecondStop { get; set; } = 5000;//(ms)
        /// <summary>
        /// Nhiệt độ sai số cọng vào thêm để alarm.
        /// </summary>
        public double ToleranceTemp { get; set; } = 2;
        /// <summary>
        /// Nhiệt độ sai số trừ vào để tắt alarm.
        /// </summary>
        public double ToleranceTempOut { get; set; } = 1;

        #region cấu hình cho UI
        /// <summary>
        /// Timer quét ở trang chính.(ms).
        /// </summary>
        public int RefreshInterval { get; set; }
        /// <summary>
        /// timer quét oe dialogPage realtime chart (ms).
        /// </summary>
        public int ChartRefreshInterval { get; set; }
        /// <summary>
        /// số điểm trên realtime chart
        /// </summary>
        public int ChartPointNum { get; set; }
        public bool Smooth = false;
        public bool ShowDataLabels = false;
        public bool ShowMarkers = true;

        public bool ChangePassUser { get; set; } = false;
        #endregion
    }
}
