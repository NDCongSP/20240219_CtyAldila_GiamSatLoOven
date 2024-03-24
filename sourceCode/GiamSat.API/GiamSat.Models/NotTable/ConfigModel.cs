﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class ConfigModel
    {
        /// <summary>
        /// Độ trễ thời gian cảnh báo (s).
        /// </summary>
        public int DeadbandAlarm { get; set; }
        /// <summary>
        /// Quy định số lẻ nhiệt độ, nhân với giá trị nhiệt độ đọc về để có số lẻ hay không.
        /// </summary>
        public double Gain { get; set; }
        /// <summary>
        /// Chu kỳ thời gian log data (ms).
        /// </summary>
        public int DataLogInterval { get; set; }
        /// <summary>
        /// Chu kỳ thời gian log data khi chạy profile.
        /// </summary>
        public int DataLogWhenRunProfileInterval { get; set; }

        #region cấu hình cho UI
        /// <summary>
        /// Timer quét ở trang chính
        /// </summary>
        public int RefreshInterval { get; set; }
        /// <summary>
        /// timer quét oe dialogPage realtime chảt
        /// </summary>
        public int ChartRefreshInterval { get; set; }
        /// <summary>
        /// số điểm trên realtime chart
        /// </summary>
        public int ChartPointNum { get; set; }
        #endregion
    }
}