using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("AlarmLog")]
    public class AlarmLogModel
    {
        [Key]
        public Guid Id { get; set; }
        public string? TagName { get; set; }
        public string? Value { get; set; }
        public string? Description { get; set; }
        public bool ACK { get; set; } = false;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ACKDate { get; set; } = DateTime.Now;
        public bool IsActived { get; set; } = true;
    }
}
