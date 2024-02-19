using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("DataLog")]
    public class DataLogModel
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChuongId { get; set; }
        public string TenChuong { get; set; }
        public double NhietDo { get; set; }
        public double DoAm { get; set; }
        public double Frequency { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }

    }
}
