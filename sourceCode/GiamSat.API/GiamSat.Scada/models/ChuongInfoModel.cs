using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Scada
{
    public class ChuongInfoModel
    {
        public Guid Id { get; set; }
        public string TenChuong { get; set; }
        public string ConfigSettings { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int NumIndex { get; set; }//đánh số thứ tự để hiển thị
        /// <summary>
        /// Chốt báo có cài đặt mới từ web xuống, để app winform biết lấy cài xuống HMI.
        /// </summary>
        public int FlagUpdate { get; set; }
    }
}
