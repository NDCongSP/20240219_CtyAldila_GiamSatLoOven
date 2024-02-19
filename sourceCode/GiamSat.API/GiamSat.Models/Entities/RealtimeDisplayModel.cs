using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("RealtimeDisplay")]
    public class RealtimeDisplayModel
    {
        [Key]
        public Guid Id { get; set; }
        public string? DisplayData { get; set; }
        public bool? IsActived { get; set; } = true;
        public DateTime? CreatedDate { get; set; }=DateTime.Now;
        public string? CreatedMachine { get; set; }
    }
}
