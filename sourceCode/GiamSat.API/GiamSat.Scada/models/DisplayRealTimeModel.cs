using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Scada
{
    [Table("DisplayRealtime")]
   public  class DisplayRealTimeModel
    {
        [Key]
        public  Guid ChuongId { get; set; }
        public string TenChuong { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Frequency { get; set; }
        public int Fan1Status { get; set; }
        public int Fan2Status { get; set; }
        public int Fan3Status { get; set; }
        public int Fan4Status { get; set; }
        public int CoollerStatus { get; set; }
        /// <summary>
        /// Báo trạng thái kết nối đến tủ.
        /// </summary>
        public string ConnectStatus { get; set; }
        public string ActiveStatus { get; set; }//báo trạng thái hoạt động là auto hay man hay dung
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public double HightTemperature { get; set; }
        public double Lowtemperature { get; set; }
        public int NumIndex { get; set; }
        /// <summary>
        /// Ngày hiện tại của giai đoạn.
        /// </summary>
        public int CurrentDay { get; set; }
        /// <summary>
        /// Giai đoạn hiện tại.
        /// </summary>
        public int CurrentStep { get; set; }
        public int TotalDay { get; set; }
        public int QuaTai { get; set; }
        public int ChayDuPhong { get; set; }
    }
}
