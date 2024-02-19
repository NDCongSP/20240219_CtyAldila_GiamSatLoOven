using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Scada
{
    /// <summary>
    /// Model để cài đặt các thông số, cài quạt chạy tương ứng với các vị trí quạt lần lượt từ 1 đến 4. Ví dụ: 1-Q4;2-Q4;3-Q2;4-Q1, và thông số thời gian chạy luân phiên dàn mát.
    /// </summary>
    public class GeneralStepSettingsModel
    {
        public string TenChuong { get; set; }
        public int NumIndex { get; set; }//đánh số thứ tự để hiển thị

        public int Fan1 { get; set; }
        public int Fan2 { get; set; }
        public int Fan3 { get; set; }
        public int Fan4 { get; set; }

        /// <summary>
        /// Thời gian chạy dàn mát, minute.
        /// </summary>
        public int TimeOnCooler { get; set; }
        /// <summary>
        /// Thời gian dừng dàn mát, minute.
        /// </summary>
        public int TimeOffCooler { get; set; }
        public int CurrentDay { get; set; }
        public double OffsetTemp { get; set; }
        public double DeadbandTemp { get; set; }
        /// <summary>
        /// coil 15. reset lai, chay lại từ đầu.
        /// </summary>
        public int ResetGiaiDoan { get; set; }
    }
}
